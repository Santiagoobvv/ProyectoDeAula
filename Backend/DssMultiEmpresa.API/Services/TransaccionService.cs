using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class TransaccionService : ITransaccionService
{
    private readonly ApplicationDbContext _db;

    public TransaccionService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<TransaccionResponseDto>> GetByEmpresaAsync(
        int empresaId,
        int? mes = null,
        int? anio = null,
        string? busqueda = null,
        int? cuentaContableId = null,
        string? codigoCuenta = null,
        int? terceroId = null,
        int? centroCostoId = null,
        string? tipo = null)
    {
        var query = _db.TransaccionesContables
            .Include(t => t.Categoria)
            .Include(t => t.CuentaContable)
            .Include(t => t.Tercero)
            .Include(t => t.CentroCosto)
            .Where(t => t.EmpresaId == empresaId);

        if (anio.HasValue && anio.Value > 0)
            query = query.Where(t => t.Fecha.Year == anio.Value);

        if (mes.HasValue && mes.Value > 0)
            query = query.Where(t => t.Fecha.Month == mes.Value);

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(t => t.Tipo == tipo);

        if (cuentaContableId.HasValue && cuentaContableId.Value > 0)
            query = query.Where(t => t.CuentaContableId == cuentaContableId.Value);

        if (!string.IsNullOrWhiteSpace(codigoCuenta))
            query = query.Where(t => t.CodigoCuenta == codigoCuenta || (t.CuentaContable != null && t.CuentaContable.Codigo == codigoCuenta));

        if (terceroId.HasValue && terceroId.Value > 0)
            query = query.Where(t => t.TerceroId == terceroId.Value);

        if (centroCostoId.HasValue && centroCostoId.Value > 0)
            query = query.Where(t => t.CentroCostoId == centroCostoId.Value);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(t =>
                t.Concepto.ToLower().Contains(term) ||
                (t.Descripcion != null && t.Descripcion.ToLower().Contains(term)) ||
                (t.NumeroTransaccion != null && t.NumeroTransaccion.ToLower().Contains(term)) ||
                (t.CodigoCuenta != null && t.CodigoCuenta.ToLower().Contains(term)) ||
                (t.NombreCuenta != null && t.NombreCuenta.ToLower().Contains(term)) ||
                (t.TerceroNit != null && t.TerceroNit.ToLower().Contains(term)) ||
                (t.TerceroNumeroDoc != null && t.TerceroNumeroDoc.ToLower().Contains(term)) ||
                (t.TerceroTipoDoc != null && t.TerceroTipoDoc.ToLower().Contains(term)) ||
                (t.TerceroNombre != null && t.TerceroNombre.ToLower().Contains(term)) ||
                (t.NumeroDocumentoSoporte != null && t.NumeroDocumentoSoporte.ToLower().Contains(term))
            );
        }

        return await query
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TransaccionResponseDto?> GetByIdAsync(int id, int empresaId)
    {
        var t = await _db.TransaccionesContables
            .Include(tr => tr.Categoria)
            .Include(tr => tr.CuentaContable)
            .Include(tr => tr.Tercero)
            .Include(tr => tr.CentroCosto)
            .FirstOrDefaultAsync(tr => tr.Id == id && tr.EmpresaId == empresaId);

        return t is null ? null : ToDto(t);
    }

    public async Task<TransaccionResponseDto> CreateAsync(int empresaId, TransaccionCreateDto dto)
    {
        if (dto.Monto <= 0 && (dto.MontoDebito ?? 0) <= 0 && (dto.MontoCredito ?? 0) <= 0)
            throw new ArgumentException("El monto de la transacción debe ser superior a cero.");

        // Determinar monto real
        decimal monto = dto.Monto;
        if (monto <= 0)
        {
            monto = Math.Max(dto.MontoDebito ?? 0, dto.MontoCredito ?? 0);
        }

        // ── 1. Validación de Cuenta Contable PUC ──
        CuentaContable? cuenta = null;
        if (dto.CuentaContableId.HasValue && dto.CuentaContableId.Value > 0)
        {
            cuenta = await _db.CuentasContables
                .FirstOrDefaultAsync(c => c.Id == dto.CuentaContableId.Value && c.EmpresaId == empresaId);
        }
        else if (!string.IsNullOrWhiteSpace(dto.CodigoCuenta))
        {
            cuenta = await _db.CuentasContables
                .FirstOrDefaultAsync(c => c.Codigo == dto.CodigoCuenta.Trim() && c.EmpresaId == empresaId);
        }

        if (cuenta is not null)
        {
            // Validar que admita movimientos
            if (!cuenta.ManejaMovimientos)
            {
                throw new InvalidOperationException(
                    $"La cuenta '{cuenta.Codigo} - {cuenta.Nombre}' es una cuenta de agrupación y NO permite movimientos directos. Debe seleccionar una subcuenta o cuenta auxiliar.");
            }
        }

        // ── 2. Validación y Enlace de Tercero ──
        Tercero? tercero = null;
        if (dto.TerceroId.HasValue && dto.TerceroId.Value > 0)
        {
            tercero = await _db.Terceros
                .FirstOrDefaultAsync(t => t.Id == dto.TerceroId.Value && t.EmpresaId == empresaId);
        }
        else if (!string.IsNullOrWhiteSpace(dto.TerceroNumeroDoc) || !string.IsNullOrWhiteSpace(dto.TerceroNit))
        {
            var docTrim = (dto.TerceroNumeroDoc ?? dto.TerceroNit)!.Trim();
            tercero = await _db.Terceros
                .FirstOrDefaultAsync(t => t.EmpresaId == empresaId && t.NumeroDocumento == docTrim);

            // Si no existe pero se proporcionó nombre y tipo, se puede registrar respetando su tipo real
            if (tercero is null && !string.IsNullOrWhiteSpace(dto.TerceroNombre))
            {
                string tipoDocAuto = dto.TerceroTipoDoc ?? "CC";
                string? dvAuto = null;
                if (tipoDocAuto == "NIT")
                {
                    dvAuto = EmpresaService.CalcularDigitoVerificacion(docTrim).ToString();
                }

                tercero = new Tercero
                {
                    EmpresaId = empresaId,
                    TipoTercero = "Proveedor",
                    TipoDocumento = tipoDocAuto,
                    NumeroDocumento = docTrim,
                    DigitoVerificacion = dvAuto,
                    RazonSocial = dto.TerceroNombre.Trim(),
                    Estado = "Activo"
                };
                _db.Terceros.Add(tercero);
                await _db.SaveChangesAsync();
            }
        }

        string? tipoDoc = tercero?.TipoDocumento ?? dto.TerceroTipoDoc;
        string? numDoc = tercero?.NumeroDocumento ?? dto.TerceroNumeroDoc ?? dto.TerceroNit;
        string? dv = tercero?.DigitoVerificacion ?? dto.TerceroDigitoVerificacion;
        string? nombreTercero = tercero?.RazonSocial ?? dto.TerceroNombre;

        // Validar formato numérico del documento del tercero si corresponde
        if (!string.IsNullOrWhiteSpace(numDoc) && (tipoDoc == "NIT" || tipoDoc == "CC" || tipoDoc == "TI"))
        {
            if (!numDoc.All(char.IsDigit))
            {
                throw new ArgumentException($"El número de documento para {tipoDoc} debe ser exclusivamente numérico.");
            }
        }

        // ── 3. Validación de Centro de Costo ──
        CentroCosto? centroCosto = null;
        if (dto.CentroCostoId.HasValue && dto.CentroCostoId.Value > 0)
        {
            centroCosto = await _db.CentrosCosto
                .FirstOrDefaultAsync(cc => cc.Id == dto.CentroCostoId.Value && cc.EmpresaId == empresaId);
        }

        // ── 4. Determinar Tipo (Ingreso / Egreso) y Naturaleza (Debito / Credito) ──
        string tipo = dto.Tipo;
        string naturaleza = dto.NaturalezaMovimiento ?? (cuenta?.Naturaleza ?? "Debito");

        if (string.IsNullOrWhiteSpace(tipo))
        {
            if (cuenta != null)
            {
                tipo = (cuenta.Clase == 4 || cuenta.TipoCategoria == "Ingreso") ? "Ingreso" : "Egreso";
            }
            else
            {
                tipo = naturaleza == "Credito" ? "Ingreso" : "Egreso";
            }
        }

        if (tipo != "Ingreso" && tipo != "Egreso")
            throw new ArgumentException("El tipo de movimiento debe ser 'Ingreso' o 'Egreso'.");

        decimal montoDebito = dto.MontoDebito ?? (naturaleza == "Debito" ? monto : 0);
        decimal montoCredito = dto.MontoCredito ?? (naturaleza == "Credito" ? monto : 0);

        // ── 5. Resolver Categoría DSS para mantener sincronizado el motor DSS ──
        int? categoriaId = dto.CategoriaId;
        if (!categoriaId.HasValue && cuenta?.CategoriaId.HasValue == true)
        {
            categoriaId = cuenta.CategoriaId;
        }

        if (!categoriaId.HasValue)
        {
            // Asignar primera categoría compatible de la empresa
            var catDefecto = await _db.CategoriasContables
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId &&
                    ((tipo == "Ingreso" && c.Tipo == "Ingreso") ||
                     (tipo == "Egreso" && (c.Tipo == "Gasto Fijo" || c.Tipo == "Gasto Variable" || c.Tipo == "Costo"))));
            if (catDefecto != null) categoriaId = catDefecto.Id;
        }

        // ── 6. Generar consecutivo de transacción si no viene ──
        string numTx = dto.NumeroTransaccion?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(numTx))
        {
            int totalHoy = await _db.TransaccionesContables
                .CountAsync(t => t.EmpresaId == empresaId && t.Fecha == dto.Fecha);
            numTx = $"TX-{dto.Fecha:yyyyMMdd}-{(totalHoy + 1):D3}";
        }

        var transaccion = new TransaccionContable
        {
            EmpresaId = empresaId,
            Fecha = dto.Fecha,
            Concepto = dto.Concepto.Trim(),
            Monto = monto,
            Tipo = tipo,
            CategoriaId = categoriaId,
            Observaciones = dto.Observaciones,

            NumeroTransaccion = numTx,
            CuentaContableId = cuenta?.Id ?? dto.CuentaContableId,
            CodigoCuenta = cuenta?.Codigo ?? dto.CodigoCuenta,
            NombreCuenta = cuenta?.Nombre ?? dto.NombreCuenta,

            TerceroId = tercero?.Id ?? dto.TerceroId,
            TerceroTipoDoc = tipoDoc,
            TerceroNumeroDoc = numDoc,
            TerceroDigitoVerificacion = dv,
            TerceroNit = numDoc,
            TerceroNombre = nombreTercero,

            CentroCostoId = centroCosto?.Id ?? dto.CentroCostoId,
            CentroCostoNombre = centroCosto?.Nombre ?? dto.CentroCostoNombre,

            NaturalezaMovimiento = naturaleza,
            MontoDebito = montoDebito,
            MontoCredito = montoCredito,

            TipoDocumentoSoporte = dto.TipoDocumentoSoporte,
            NumeroDocumentoSoporte = dto.NumeroDocumentoSoporte,
            FechaDocumentoSoporte = dto.FechaDocumentoSoporte,
            Descripcion = dto.Descripcion ?? dto.Concepto
        };

        _db.TransaccionesContables.Add(transaccion);
        await _db.SaveChangesAsync();

        // Cargar referencias para el response
        if (transaccion.CategoriaId.HasValue)
            await _db.Entry(transaccion).Reference(t => t.Categoria).LoadAsync();
        if (transaccion.CuentaContableId.HasValue)
            await _db.Entry(transaccion).Reference(t => t.CuentaContable).LoadAsync();
        if (transaccion.TerceroId.HasValue)
            await _db.Entry(transaccion).Reference(t => t.Tercero).LoadAsync();
        if (transaccion.CentroCostoId.HasValue)
            await _db.Entry(transaccion).Reference(t => t.CentroCosto).LoadAsync();

        return ToDto(transaccion);
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var t = await _db.TransaccionesContables
            .FirstOrDefaultAsync(tr => tr.Id == id && tr.EmpresaId == empresaId);
        if (t is null) return false;

        _db.TransaccionesContables.Remove(t);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TransaccionResponseDto ToDto(TransaccionContable t)
    {
        var tipoDoc = t.TerceroTipoDoc ?? t.Tercero?.TipoDocumento ?? (string.IsNullOrEmpty(t.TerceroNumeroDoc ?? t.TerceroNit) ? null : "NIT");
        var numDoc = t.TerceroNumeroDoc ?? t.Tercero?.NumeroDocumento ?? t.TerceroNit;
        var dv = t.TerceroDigitoVerificacion ?? t.Tercero?.DigitoVerificacion;
        var nombreTercero = t.TerceroNombre ?? t.Tercero?.RazonSocial;

        return new TransaccionResponseDto(
            t.Id,
            t.EmpresaId,
            t.Fecha,
            t.Concepto,
            t.Monto,
            t.Tipo,
            t.CategoriaId,
            t.Categoria?.Nombre,
            t.Observaciones,
            t.NumeroTransaccion,
            t.CuentaContableId,
            t.CodigoCuenta ?? t.CuentaContable?.Codigo,
            t.NombreCuenta ?? t.CuentaContable?.Nombre,
            t.TerceroId,
            numDoc,
            nombreTercero,
            t.CentroCostoId,
            t.CentroCostoNombre ?? t.CentroCosto?.Nombre,
            t.NaturalezaMovimiento,
            t.MontoDebito,
            t.MontoCredito,
            t.TipoDocumentoSoporte,
            t.NumeroDocumentoSoporte,
            t.FechaDocumentoSoporte,
            t.Descripcion,
            tipoDoc,
            numDoc,
            dv
        );
    }
}
