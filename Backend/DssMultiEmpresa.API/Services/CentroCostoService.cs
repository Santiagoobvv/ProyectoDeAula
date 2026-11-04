using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class CentroCostoService : ICentroCostoService
{
    private readonly ApplicationDbContext _db;

    public CentroCostoService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<CentroCostoResponseDto>> GetByEmpresaAsync(int empresaId)
    {
        return await _db.CentrosCosto
            .Where(cc => cc.EmpresaId == empresaId)
            .OrderBy(cc => cc.Codigo)
            .Select(cc => ToDto(cc))
            .ToListAsync();
    }

    public async Task<CentroCostoResponseDto?> GetByIdAsync(int id, int empresaId)
    {
        var cc = await _db.CentrosCosto
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
        return cc is null ? null : ToDto(cc);
    }

    public async Task<CentroCostoResponseDto> CreateAsync(int empresaId, CentroCostoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Any(char.IsDigit))
            throw new ArgumentException("El nombre del centro de costo no debe contener números.");

        var codigo = dto.Codigo.Trim().ToUpper();
        var existe = await _db.CentrosCosto.AnyAsync(c => c.EmpresaId == empresaId && c.Codigo == codigo);
        if (existe)
            throw new InvalidOperationException($"Ya existe un centro de costo con el código '{codigo}' para esta empresa.");

        var cc = new CentroCosto
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            Estado = dto.Estado ?? "Activo"
        };

        _db.CentrosCosto.Add(cc);
        await _db.SaveChangesAsync();

        return ToDto(cc);
    }

    public async Task<CentroCostoResponseDto?> UpdateAsync(int id, int empresaId, CentroCostoUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Any(char.IsDigit))
            throw new ArgumentException("El nombre del centro de costo no debe contener números.");

        var cc = await _db.CentrosCosto
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
        if (cc is null) return null;

        var codigo = dto.Codigo.Trim().ToUpper();
        if (cc.Codigo != codigo)
        {
            var existe = await _db.CentrosCosto.AnyAsync(c => c.EmpresaId == empresaId && c.Codigo == codigo && c.Id != id);
            if (existe)
                throw new InvalidOperationException($"Ya existe otro centro de costo con el código '{codigo}'.");
        }

        cc.Codigo = codigo;
        cc.Nombre = dto.Nombre.Trim();
        cc.Descripcion = dto.Descripcion;
        cc.Estado = dto.Estado ?? cc.Estado;

        await _db.SaveChangesAsync();
        return ToDto(cc);
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var cc = await _db.CentrosCosto
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
        if (cc is null) return false;

        var tieneTransacciones = await _db.TransaccionesContables.AnyAsync(t => t.CentroCostoId == id);
        if (tieneTransacciones)
            throw new InvalidOperationException("No se puede eliminar el centro de costo porque tiene transacciones asociadas.");

        _db.CentrosCosto.Remove(cc);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> SeedCentrosCostoBaseAsync(int empresaId)
    {
        var codigosExistentes = await _db.CentrosCosto
            .Where(cc => cc.EmpresaId == empresaId)
            .Select(cc => cc.Codigo)
            .ToListAsync();
        var existentes = codigosExistentes.ToHashSet();

        var baseItems = new (string Codigo, string Nombre, string Descripcion)[]
        {
            ("CC-ADM", "Administración General", "Costos y gastos del área administrativa y gerencia."),
            ("CC-CONTAB", "Contabilidad y Tributaria", "Gestión contable, impuestos y nómina."),
            ("CC-OPER", "Operaciones y Servicios", "Prestación directa de servicios o proyectos."),
            ("CC-COM", "Comercial y Mercadeo", "Gestión comercial, clientes y ventas."),
            ("CC-TEC", "Tecnología e Infraestructura", "Sistemas, software, servidores y soporte TI.")
        };

        int count = 0;
        foreach (var (codigo, nombre, desc) in baseItems)
        {
            if (!existentes.Contains(codigo))
            {
                _db.CentrosCosto.Add(new CentroCosto
                {
                    EmpresaId = empresaId,
                    Codigo = codigo,
                    Nombre = nombre,
                    Descripcion = desc,
                    Estado = "Activo"
                });
                count++;
            }
        }

        if (count > 0)
        {
            await _db.SaveChangesAsync();
        }

        return count;
    }

    private static CentroCostoResponseDto ToDto(CentroCosto cc) => new(
        cc.Id,
        cc.EmpresaId,
        cc.Codigo,
        cc.Nombre,
        cc.Descripcion,
        cc.Estado
    );
}
