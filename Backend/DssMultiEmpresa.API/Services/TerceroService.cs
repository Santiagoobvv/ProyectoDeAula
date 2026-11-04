using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class TerceroService : ITerceroService
{
    private readonly ApplicationDbContext _db;

    public TerceroService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<TerceroResponseDto>> GetByEmpresaAsync(
        int empresaId, string? busqueda = null, string? tipoTercero = null)
    {
        var query = _db.Terceros.Where(t => t.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(t =>
                t.NumeroDocumento.ToLower().Contains(term) ||
                t.RazonSocial.ToLower().Contains(term) ||
                (t.NombreComercial != null && t.NombreComercial.ToLower().Contains(term)) ||
                (t.PrimerNombre != null && t.PrimerNombre.ToLower().Contains(term)) ||
                (t.PrimerApellido != null && t.PrimerApellido.ToLower().Contains(term))
            );
        }

        if (!string.IsNullOrWhiteSpace(tipoTercero))
        {
            query = query.Where(t => t.TipoTercero == tipoTercero);
        }

        return await query
            .OrderBy(t => t.RazonSocial)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TerceroResponseDto?> GetByIdAsync(int id, int empresaId)
    {
        var tercero = await _db.Terceros
            .FirstOrDefaultAsync(t => t.Id == id && t.EmpresaId == empresaId);
        return tercero is null ? null : ToDto(tercero);
    }

    public async Task<TerceroResponseDto?> GetByDocumentoAsync(string numeroDocumento, int empresaId)
    {
        var doc = numeroDocumento.Trim();
        var tercero = await _db.Terceros
            .FirstOrDefaultAsync(t => t.EmpresaId == empresaId && t.NumeroDocumento == doc);
        return tercero is null ? null : ToDto(tercero);
    }

    public async Task<TerceroResponseDto> CreateAsync(int empresaId, TerceroCreateDto dto)
    {
        ValidarDatosTercero(dto);
        var doc = dto.NumeroDocumento.Trim();
        var existe = await _db.Terceros.AnyAsync(t => t.EmpresaId == empresaId && t.NumeroDocumento == doc);
        if (existe)
            throw new InvalidOperationException($"Ya existe un tercero registrado con el documento '{doc}' para esta empresa.");

        string? dv = dto.DigitoVerificacion;
        if (dto.TipoDocumento == "NIT" && !string.IsNullOrWhiteSpace(doc))
        {
            dv = EmpresaService.CalcularDigitoVerificacion(doc).ToString();
        }

        string razonSocial = dto.RazonSocial;
        if (string.IsNullOrWhiteSpace(razonSocial))
        {
            razonSocial = $"{dto.PrimerNombre} {dto.PrimerApellido}".Trim();
            if (string.IsNullOrWhiteSpace(razonSocial)) razonSocial = "Tercero sin nombre";
        }

        var tercero = new Tercero
        {
            EmpresaId = empresaId,
            TipoTercero = dto.TipoTercero ?? "Proveedor",
            TipoDocumento = dto.TipoDocumento ?? "NIT",
            NumeroDocumento = doc,
            DigitoVerificacion = dv,
            RazonSocial = razonSocial,
            NombreComercial = dto.NombreComercial,
            PrimerNombre = dto.PrimerNombre,
            PrimerApellido = dto.PrimerApellido,
            Departamento = dto.Departamento,
            Municipio = dto.Municipio,
            Direccion = dto.Direccion,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Celular = dto.Celular,
            Estado = dto.Estado ?? "Activo",
            Observaciones = dto.Observaciones
        };

        _db.Terceros.Add(tercero);
        await _db.SaveChangesAsync();

        return ToDto(tercero);
    }

    public async Task<TerceroResponseDto?> UpdateAsync(int id, int empresaId, TerceroUpdateDto dto)
    {
        ValidarDatosTercero(dto);
        var tercero = await _db.Terceros
            .FirstOrDefaultAsync(t => t.Id == id && t.EmpresaId == empresaId);
        if (tercero is null) return null;

        var doc = dto.NumeroDocumento.Trim();
        if (tercero.NumeroDocumento != doc)
        {
            var existe = await _db.Terceros.AnyAsync(t => t.EmpresaId == empresaId && t.NumeroDocumento == doc && t.Id != id);
            if (existe)
                throw new InvalidOperationException($"Ya existe otro tercero con el documento '{doc}'.");
        }

        string? dv = dto.DigitoVerificacion;
        if (dto.TipoDocumento == "NIT" && !string.IsNullOrWhiteSpace(doc))
        {
            dv = EmpresaService.CalcularDigitoVerificacion(doc).ToString();
        }

        tercero.TipoTercero = dto.TipoTercero ?? tercero.TipoTercero;
        tercero.TipoDocumento = dto.TipoDocumento ?? tercero.TipoDocumento;
        tercero.NumeroDocumento = doc;
        tercero.DigitoVerificacion = dv;
        tercero.RazonSocial = dto.RazonSocial;
        tercero.NombreComercial = dto.NombreComercial;
        tercero.PrimerNombre = dto.PrimerNombre;
        tercero.PrimerApellido = dto.PrimerApellido;
        tercero.Departamento = dto.Departamento;
        tercero.Municipio = dto.Municipio;
        tercero.Direccion = dto.Direccion;
        tercero.Email = dto.Email;
        tercero.Telefono = dto.Telefono;
        tercero.Celular = dto.Celular;
        tercero.Estado = dto.Estado ?? tercero.Estado;
        tercero.Observaciones = dto.Observaciones;

        await _db.SaveChangesAsync();
        return ToDto(tercero);
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var tercero = await _db.Terceros
            .FirstOrDefaultAsync(t => t.Id == id && t.EmpresaId == empresaId);
        if (tercero is null) return false;

        var tieneTransacciones = await _db.TransaccionesContables.AnyAsync(t => t.TerceroId == id);
        if (tieneTransacciones)
            throw new InvalidOperationException("No se puede eliminar el tercero porque tiene movimientos contables vinculados. Puede marcarlo como inactivo.");

        _db.Terceros.Remove(tercero);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TerceroResponseDto ToDto(Tercero t) => new(
        t.Id,
        t.EmpresaId,
        t.TipoTercero,
        t.TipoDocumento,
        t.NumeroDocumento,
        t.DigitoVerificacion,
        t.RazonSocial,
        t.NombreComercial,
        t.PrimerNombre,
        t.PrimerApellido,
        t.Departamento,
        t.Municipio,
        t.Direccion,
        t.Email,
        t.Telefono,
        t.Celular,
        t.Estado,
        t.Observaciones
    );

    private static void ValidarDatosTercero(dynamic dto)
    {
        string tipoDoc = dto.TipoDocumento ?? "NIT";
        string doc = ((string)dto.NumeroDocumento).Trim();

        if (tipoDoc == "NIT" || tipoDoc == "CC" || tipoDoc == "TI")
        {
            if (!doc.All(char.IsDigit))
                throw new ArgumentException($"El número de documento para {tipoDoc} debe ser exclusivamente numérico.");
        }

        if (!string.IsNullOrWhiteSpace(dto.DigitoVerificacion) && !((string)dto.DigitoVerificacion).All(char.IsDigit))
            throw new ArgumentException("El dígito de verificación debe ser un número.");

        if (!string.IsNullOrWhiteSpace(dto.RazonSocial) && ((string)dto.RazonSocial).Any(char.IsDigit))
            throw new ArgumentException("La razón social solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.NombreComercial) && ((string)dto.NombreComercial).Any(char.IsDigit))
            throw new ArgumentException("El nombre comercial solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.PrimerNombre) && ((string)dto.PrimerNombre).Any(char.IsDigit))
            throw new ArgumentException("El primer nombre solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.PrimerApellido) && ((string)dto.PrimerApellido).Any(char.IsDigit))
            throw new ArgumentException("El primer apellido solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Departamento) && ((string)dto.Departamento).Any(char.IsDigit))
            throw new ArgumentException("El departamento solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Municipio) && ((string)dto.Municipio).Any(char.IsDigit))
            throw new ArgumentException("El municipio solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Celular) && !((string)dto.Celular).All(char.IsDigit))
            throw new ArgumentException("El teléfono celular solo debe contener números.");
    }
}
