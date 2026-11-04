using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class PresupuestoService : IPresupuestoService
{
    private readonly ApplicationDbContext _db;

    public PresupuestoService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<PresupuestoResponseDto>> GetByEmpresaAsync(
        int empresaId,
        int? mes = null,
        int? anio = null,
        string? busqueda = null,
        string? categoria = null,
        string? tipoPresupuesto = null,
        string? estado = null)
    {
        var query = _db.Presupuestos
            .Include(p => p.Categoria)
            .Include(p => p.CuentaContable)
            .Where(p => p.EmpresaId == empresaId);

        if (anio.HasValue && anio.Value > 0)
            query = query.Where(p => p.PeriodoAnio == anio.Value);

        if (mes.HasValue && mes.Value > 0)
            query = query.Where(p => p.PeriodoMes == mes.Value);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(p =>
                (p.Codigo != null && p.Codigo.ToLower().Contains(term)) ||
                (p.Nombre != null && p.Nombre.ToLower().Contains(term)) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(term)) ||
                (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(term)) ||
                (p.CuentaContable != null && (p.CuentaContable.Codigo.ToLower().Contains(term) || p.CuentaContable.Nombre.ToLower().Contains(term))) ||
                (p.CategoriaNombre != null && p.CategoriaNombre.ToLower().Contains(term))
            );
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var cat = categoria.Trim().ToLower();
            query = query.Where(p =>
                (p.CategoriaNombre != null && p.CategoriaNombre.ToLower().Contains(cat)) ||
                (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(cat))
            );
        }

        if (!string.IsNullOrWhiteSpace(tipoPresupuesto))
        {
            query = query.Where(p => p.TipoPresupuesto == tipoPresupuesto);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(p => p.Estado == estado);
        }

        return await query
            .OrderBy(p => p.PeriodoAnio)
            .ThenBy(p => p.PeriodoMes)
            .ThenBy(p => p.Codigo)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<PresupuestoResponseDto?> GetByIdAsync(int id, int empresaId)
    {
        var p = await _db.Presupuestos
            .Include(pr => pr.Categoria)
            .Include(pr => pr.CuentaContable)
            .FirstOrDefaultAsync(pr => pr.Id == id && pr.EmpresaId == empresaId);

        return p is null ? null : ToDto(p);
    }

    public async Task<PresupuestoResponseDto> CreateAsync(int empresaId, PresupuestoCreateDto dto)
    {
        if (dto.MontoPresupuestado < 0)
            throw new ArgumentException("El monto presupuestado no puede ser un valor negativo.");

        if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre.Trim().Any(char.IsDigit))
            throw new ArgumentException("El nombre de la partida presupuestal solo debe contener letras.");

        // Validar categoría macro perteneciente a la empresa
        var categoria = await _db.CategoriasContables.FirstOrDefaultAsync(c =>
            c.Id == dto.CategoriaId && c.EmpresaId == empresaId);
        if (categoria is null)
            throw new ArgumentException("La categoría contable especificada no pertenece a esta empresa o no existe.");

        // Validar cuenta contable si fue proporcionada
        if (dto.CuentaContableId.HasValue)
        {
            var cuenta = await _db.CuentasContables.FirstOrDefaultAsync(c =>
                c.Id == dto.CuentaContableId.Value && c.EmpresaId == empresaId);
            if (cuenta is null)
                throw new ArgumentException("La cuenta contable especificada no pertenece a esta empresa o no existe.");
        }

        // Generar o validar código único por empresa
        string codigo = string.IsNullOrWhiteSpace(dto.Codigo)
            ? $"PRE-{dto.PeriodoAnio}-{dto.PeriodoMes:D2}-{(dto.CuentaContableId.HasValue ? dto.CuentaContableId.Value : dto.CategoriaId)}"
            : dto.Codigo.Trim().ToUpper();

        var existeCodigo = await _db.Presupuestos.AnyAsync(p =>
            p.EmpresaId == empresaId && p.Codigo == codigo);

        if (existeCodigo)
            throw new InvalidOperationException($"Ya existe un presupuesto con el código '{codigo}' en esta empresa.");

        // Verificar unicidad de la combinación mes/año/categoría para la regla del índice único
        var existeCombinacion = await _db.Presupuestos.AnyAsync(p =>
            p.EmpresaId == empresaId &&
            p.PeriodoAnio == dto.PeriodoAnio &&
            p.PeriodoMes == dto.PeriodoMes &&
            p.CategoriaId == dto.CategoriaId);

        if (existeCombinacion)
            throw new InvalidOperationException("Ya existe un presupuesto para esta categoría macro en el periodo seleccionado.");

        var presupuesto = new Presupuesto
        {
            EmpresaId = empresaId,
            PeriodoMes = dto.PeriodoMes,
            PeriodoAnio = dto.PeriodoAnio,
            CategoriaId = dto.CategoriaId,
            MontoPresupuestado = dto.MontoPresupuestado,
            Codigo = codigo,
            Nombre = dto.Nombre ?? $"Presupuesto {dto.CategoriaNombre ?? "General"} {dto.PeriodoMes:D2}/{dto.PeriodoAnio}",
            Descripcion = dto.Descripcion,
            CuentaContableId = dto.CuentaContableId,
            TipoPresupuesto = dto.TipoPresupuesto ?? "Gasto Fijo",
            CategoriaNombre = dto.CategoriaNombre ?? "Operación",
            Estado = dto.Estado ?? "Activo",
            Observaciones = dto.Observaciones
        };

        _db.Presupuestos.Add(presupuesto);
        await _db.SaveChangesAsync();

        await _db.Entry(presupuesto).Reference(p => p.Categoria).LoadAsync();
        if (presupuesto.CuentaContableId.HasValue)
        {
            await _db.Entry(presupuesto).Reference(p => p.CuentaContable).LoadAsync();
        }

        return ToDto(presupuesto);
    }

    public async Task<PresupuestoResponseDto?> UpdateAsync(int id, int empresaId, PresupuestoUpdateDto dto)
    {
        var p = await _db.Presupuestos
            .Include(pr => pr.Categoria)
            .Include(pr => pr.CuentaContable)
            .FirstOrDefaultAsync(pr => pr.Id == id && pr.EmpresaId == empresaId);

        if (p is null) return null;

        if (dto.MontoPresupuestado < 0)
            throw new ArgumentException("El monto presupuestado no puede ser un valor negativo.");

        if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre.Trim().Any(char.IsDigit))
            throw new ArgumentException("El nombre de la partida presupuestal solo debe contener letras.");

        var categoria = await _db.CategoriasContables.FirstOrDefaultAsync(c =>
            c.Id == dto.CategoriaId && c.EmpresaId == empresaId);
        if (categoria is null)
            throw new ArgumentException("La categoría contable especificada no pertenece a esta empresa o no existe.");

        if (dto.CuentaContableId.HasValue)
        {
            var cuenta = await _db.CuentasContables.FirstOrDefaultAsync(c =>
                c.Id == dto.CuentaContableId.Value && c.EmpresaId == empresaId);
            if (cuenta is null)
                throw new ArgumentException("La cuenta contable especificada no pertenece a esta empresa o no existe.");
        }

        string codigo = string.IsNullOrWhiteSpace(dto.Codigo) ? p.Codigo ?? $"PRE-{id}" : dto.Codigo.Trim().ToUpper();

        var existeCodigo = await _db.Presupuestos.AnyAsync(pr =>
            pr.EmpresaId == empresaId && pr.Codigo == codigo && pr.Id != id);

        if (existeCodigo)
            throw new InvalidOperationException($"Ya existe otro presupuesto con el código '{codigo}'.");

        p.PeriodoMes = dto.PeriodoMes;
        p.PeriodoAnio = dto.PeriodoAnio;
        p.CategoriaId = dto.CategoriaId;
        p.MontoPresupuestado = dto.MontoPresupuestado;
        p.Codigo = codigo;
        p.Nombre = dto.Nombre ?? p.Nombre;
        p.Descripcion = dto.Descripcion;
        p.CuentaContableId = dto.CuentaContableId;
        p.TipoPresupuesto = dto.TipoPresupuesto ?? p.TipoPresupuesto;
        p.CategoriaNombre = dto.CategoriaNombre ?? p.CategoriaNombre;
        p.Estado = dto.Estado ?? p.Estado;
        p.Observaciones = dto.Observaciones;

        await _db.SaveChangesAsync();

        if (p.CuentaContableId.HasValue && p.CuentaContable == null)
        {
            await _db.Entry(p).Reference(pr => pr.CuentaContable).LoadAsync();
        }

        return ToDto(p);
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var p = await _db.Presupuestos
            .FirstOrDefaultAsync(pr => pr.Id == id && pr.EmpresaId == empresaId);
        if (p is null) return false;

        _db.Presupuestos.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }

    private static PresupuestoResponseDto ToDto(Presupuesto p) => new(
        p.Id,
        p.EmpresaId,
        p.PeriodoMes,
        p.PeriodoAnio,
        p.CategoriaId,
        p.Categoria?.Nombre,
        p.MontoPresupuestado,
        p.Codigo,
        p.Nombre,
        p.Descripcion,
        p.CuentaContableId,
        p.CuentaContable?.Codigo,
        p.CuentaContable?.Nombre,
        p.TipoPresupuesto,
        p.CategoriaNombre,
        p.Estado,
        p.Observaciones
    );
}
