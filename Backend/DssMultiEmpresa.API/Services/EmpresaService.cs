using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class EmpresaService : IEmpresaService
{
    private readonly ApplicationDbContext _db;

    public EmpresaService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<EmpresaResponseDto>> GetByUsuarioAsync(int usuarioId)
    {
        return await _db.Empresas
            .Where(e => e.UsuarioId == usuarioId)
            .OrderBy(e => e.NombreEmpresa)
            .Select(e => ToDto(e))
            .ToListAsync();
    }

    public async Task<EmpresaResponseDto?> GetByIdAsync(int id, int usuarioId)
    {
        var empresa = await _db.Empresas
            .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == usuarioId);
        return empresa is null ? null : ToDto(empresa);
    }

    public async Task<EmpresaResponseDto> CreateAsync(int usuarioId, EmpresaCreateDto dto)
    {
        ValidarDatosEmpresa(dto);
        var (nombreEmpresa, nit, dv) = NormalizarNombreYDocumento(dto);

        var empresa = new Empresa
        {
            UsuarioId = usuarioId,
            NombreEmpresa = nombreEmpresa,
            Nit = nit,
            Sector = dto.Sector,
            Descripcion = dto.Descripcion,
            FechaCreacion = DateTime.UtcNow,

            TipoPersona = dto.TipoPersona ?? "Juridica",
            TipoDocumento = dto.TipoDocumento ?? "NIT",
            NumeroDocumento = dto.NumeroDocumento,
            DigitoVerificacion = dv,
            RazonSocial = dto.RazonSocial,
            NombreComercial = dto.NombreComercial,
            PrimerNombre = dto.PrimerNombre,
            SegundoNombre = dto.SegundoNombre,
            PrimerApellido = dto.PrimerApellido,
            SegundoApellido = dto.SegundoApellido,
            Estado = dto.Estado ?? "Activa",

            Pais = dto.Pais ?? "Colombia",
            Departamento = dto.Departamento,
            Municipio = dto.Municipio,
            DireccionPrincipal = dto.DireccionPrincipal,
            ComplementoDireccion = dto.ComplementoDireccion,
            Barrio = dto.Barrio,
            CodigoPostal = dto.CodigoPostal,
            EmailContacto = dto.EmailContacto,
            TelefonoFijo = dto.TelefonoFijo,
            Celular = dto.Celular,
            SitioWeb = dto.SitioWeb,
            DireccionCorrespondencia = dto.DireccionCorrespondencia,

            ResponsableNombre = dto.ResponsableNombre,
            ResponsableTipoDocumento = dto.ResponsableTipoDocumento,
            ResponsableDocumento = dto.ResponsableDocumento,
            ResponsableCargo = dto.ResponsableCargo,
            ResponsableRol = dto.ResponsableRol ?? "Representante Legal",
            ResponsableEmail = dto.ResponsableEmail,
            ResponsableTelefono = dto.ResponsableTelefono,
            ResponsableObservaciones = dto.ResponsableObservaciones,

            RegimenTributario = dto.RegimenTributario ?? "Responsable de IVA",
            ResponsabilidadIva = dto.ResponsabilidadIva ?? "Responsable",
            CodigoCiiu = dto.CodigoCiiu,
            ActividadEconomica = dto.ActividadEconomica,
            ActividadesSecundarias = dto.ActividadesSecundarias,
            ResponsabilidadesTributarias = dto.ResponsabilidadesTributarias,
            ObservacionesTributarias = dto.ObservacionesTributarias
        };

        _db.Empresas.Add(empresa);
        await _db.SaveChangesAsync();

        // Inicializar categorías base para DSS si no existen
        await InicializarCategoriasDssAsync(empresa.Id);

        return ToDto(empresa);
    }

    public async Task<EmpresaResponseDto?> UpdateAsync(int id, int usuarioId, EmpresaUpdateDto dto)
    {
        ValidarDatosEmpresa(dto);
        var empresa = await _db.Empresas
            .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == usuarioId);

        if (empresa is null) return null;

        var (nombreEmpresa, nit, dv) = NormalizarNombreYDocumento(dto);

        empresa.NombreEmpresa = nombreEmpresa;
        empresa.Nit = nit;
        empresa.Sector = dto.Sector;
        empresa.Descripcion = dto.Descripcion;

        empresa.TipoPersona = dto.TipoPersona ?? empresa.TipoPersona;
        empresa.TipoDocumento = dto.TipoDocumento ?? empresa.TipoDocumento;
        empresa.NumeroDocumento = dto.NumeroDocumento;
        empresa.DigitoVerificacion = dv;
        empresa.RazonSocial = dto.RazonSocial;
        empresa.NombreComercial = dto.NombreComercial;
        empresa.PrimerNombre = dto.PrimerNombre;
        empresa.SegundoNombre = dto.SegundoNombre;
        empresa.PrimerApellido = dto.PrimerApellido;
        empresa.SegundoApellido = dto.SegundoApellido;
        empresa.Estado = dto.Estado ?? empresa.Estado;

        empresa.Pais = dto.Pais ?? empresa.Pais;
        empresa.Departamento = dto.Departamento;
        empresa.Municipio = dto.Municipio;
        empresa.DireccionPrincipal = dto.DireccionPrincipal;
        empresa.ComplementoDireccion = dto.ComplementoDireccion;
        empresa.Barrio = dto.Barrio;
        empresa.CodigoPostal = dto.CodigoPostal;
        empresa.EmailContacto = dto.EmailContacto;
        empresa.TelefonoFijo = dto.TelefonoFijo;
        empresa.Celular = dto.Celular;
        empresa.SitioWeb = dto.SitioWeb;
        empresa.DireccionCorrespondencia = dto.DireccionCorrespondencia;

        empresa.ResponsableNombre = dto.ResponsableNombre;
        empresa.ResponsableTipoDocumento = dto.ResponsableTipoDocumento;
        empresa.ResponsableDocumento = dto.ResponsableDocumento;
        empresa.ResponsableCargo = dto.ResponsableCargo;
        empresa.ResponsableRol = dto.ResponsableRol ?? empresa.ResponsableRol;
        empresa.ResponsableEmail = dto.ResponsableEmail;
        empresa.ResponsableTelefono = dto.ResponsableTelefono;
        empresa.ResponsableObservaciones = dto.ResponsableObservaciones;

        empresa.RegimenTributario = dto.RegimenTributario ?? empresa.RegimenTributario;
        empresa.ResponsabilidadIva = dto.ResponsabilidadIva ?? empresa.ResponsabilidadIva;
        empresa.CodigoCiiu = dto.CodigoCiiu;
        empresa.ActividadEconomica = dto.ActividadEconomica;
        empresa.ActividadesSecundarias = dto.ActividadesSecundarias;
        empresa.ResponsabilidadesTributarias = dto.ResponsabilidadesTributarias;
        empresa.ObservacionesTributarias = dto.ObservacionesTributarias;

        await _db.SaveChangesAsync();
        return ToDto(empresa);
    }

    public async Task<bool> DeleteAsync(int id, int usuarioId)
    {
        var empresa = await _db.Empresas
            .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == usuarioId);

        if (empresa is null) return false;

        _db.Empresas.Remove(empresa);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PerteneceAUsuarioAsync(int empresaId, int usuarioId)
    {
        return await _db.Empresas.AnyAsync(e => e.Id == empresaId && e.UsuarioId == usuarioId);
    }

    private static void ValidarDatosEmpresa(dynamic dto)
    {
        string tipoDoc = dto.TipoDocumento ?? "NIT";
        string? numDoc = dto.NumeroDocumento ?? dto.Nit;
        if (!string.IsNullOrWhiteSpace(numDoc) && (tipoDoc == "NIT" || tipoDoc == "CC" || tipoDoc == "TI"))
        {
            if (!numDoc.All(char.IsDigit))
                throw new ArgumentException($"El número de identificación para {tipoDoc} debe contener exclusivamente números.");
        }

        if (!string.IsNullOrWhiteSpace(dto.DigitoVerificacion) && !((string)dto.DigitoVerificacion).All(char.IsDigit))
            throw new ArgumentException("El dígito de verificación debe ser un número.");

        if (!string.IsNullOrWhiteSpace(dto.RazonSocial) && ((string)dto.RazonSocial).Any(char.IsDigit))
            throw new ArgumentException("La razón social solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.NombreComercial) && ((string)dto.NombreComercial).Any(char.IsDigit))
            throw new ArgumentException("El nombre comercial solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.PrimerNombre) && ((string)dto.PrimerNombre).Any(char.IsDigit))
            throw new ArgumentException("El primer nombre solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.SegundoNombre) && ((string)dto.SegundoNombre).Any(char.IsDigit))
            throw new ArgumentException("El segundo nombre solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.PrimerApellido) && ((string)dto.PrimerApellido).Any(char.IsDigit))
            throw new ArgumentException("El primer apellido solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.SegundoApellido) && ((string)dto.SegundoApellido).Any(char.IsDigit))
            throw new ArgumentException("El segundo apellido solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Sector) && ((string)dto.Sector).Any(char.IsDigit))
            throw new ArgumentException("El sector económico solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Pais) && ((string)dto.Pais).Any(char.IsDigit))
            throw new ArgumentException("El país solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Municipio) && ((string)dto.Municipio).Any(char.IsDigit))
            throw new ArgumentException("El municipio solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Celular) && !((string)dto.Celular).All(char.IsDigit))
            throw new ArgumentException("El número celular solo debe contener números.");

        if (!string.IsNullOrWhiteSpace(dto.CodigoPostal) && !((string)dto.CodigoPostal).All(char.IsDigit))
            throw new ArgumentException("El código postal solo debe contener números.");

        if (!string.IsNullOrWhiteSpace(dto.ResponsableNombre) && ((string)dto.ResponsableNombre).Any(char.IsDigit))
            throw new ArgumentException("El nombre del responsable solo debe contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.ResponsableTelefono) && !((string)dto.ResponsableTelefono).All(char.IsDigit))
            throw new ArgumentException("El teléfono del responsable solo debe contener números.");

        if (!string.IsNullOrWhiteSpace(dto.ResponsableDocumento) && !((string)dto.ResponsableDocumento).All(char.IsDigit))
            throw new ArgumentException("El documento del responsable solo debe contener números.");

        if (!string.IsNullOrWhiteSpace(dto.CodigoCiiu) && !((string)dto.CodigoCiiu).All(char.IsDigit))
            throw new ArgumentException("El código CIIU principal solo debe contener números.");
    }

    private static (string nombre, string? nit, string? dv) NormalizarNombreYDocumento(dynamic dto)
    {
        string nombre = dto.NombreEmpresa;
        if (string.IsNullOrWhiteSpace(nombre))
        {
            if (!string.IsNullOrWhiteSpace(dto.RazonSocial))
                nombre = dto.RazonSocial;
            else if (!string.IsNullOrWhiteSpace(dto.PrimerNombre) || !string.IsNullOrWhiteSpace(dto.PrimerApellido))
                nombre = $"{dto.PrimerNombre} {dto.SegundoNombre} {dto.PrimerApellido} {dto.SegundoApellido}".Replace("  ", " ").Trim();
            else
                nombre = "Empresa sin nombre";
        }

        string? doc = dto.NumeroDocumento ?? dto.Nit;
        string? dv = dto.DigitoVerificacion;

        if (dto.TipoDocumento == "NIT" && !string.IsNullOrWhiteSpace(doc))
        {
            dv = CalcularDigitoVerificacion(doc).ToString();
        }

        string? nitFormateado = doc;
        if (!string.IsNullOrWhiteSpace(doc) && !string.IsNullOrWhiteSpace(dv))
        {
            nitFormateado = $"{doc}-{dv}";
        }

        return (nombre, nitFormateado, dv);
    }

    public static int CalcularDigitoVerificacion(string nit)
    {
        if (string.IsNullOrWhiteSpace(nit)) return 0;
        string digitos = new string(nit.Where(char.IsDigit).ToArray());
        if (digitos.Length == 0) return 0;

        int[] factores = { 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
        int suma = 0;
        for (int i = 0; i < digitos.Length && i < factores.Length; i++)
        {
            int digito = digitos[digitos.Length - 1 - i] - '0';
            suma += digito * factores[i];
        }
        int residuo = suma % 11;
        if (residuo == 0) return 0;
        if (residuo == 1) return 1;
        return 11 - residuo;
    }

    private async Task InicializarCategoriasDssAsync(int empresaId)
    {
        var tieneCategorias = await _db.CategoriasContables.AnyAsync(c => c.EmpresaId == empresaId);
        if (tieneCategorias) return;

        var categoriasBase = new (string Nombre, string Tipo)[]
        {
            ("Ingresos por Honorarios y Asesorías", "Ingreso"),
            ("Ingresos por Servicios Contables", "Ingreso"),
            ("Costos de Operación y Personal", "Costo"),
            ("Nómina y Prestaciones Sociales", "Gasto Fijo"),
            ("Arrendamientos y Servicios Públicos", "Gasto Fijo"),
            ("Software y Suscripciones Tecnológicas", "Gasto Fijo"),
            ("Capacitación y Formación Profesional", "Gasto Variable"),
            ("Papelería, Suministros y Transporte", "Gasto Variable"),
            ("Gastos Bancarios y Financieros", "Gasto Variable")
        };

        foreach (var (nombre, tipo) in categoriasBase)
        {
            _db.CategoriasContables.Add(new CategoriaContable
            {
                EmpresaId = empresaId,
                Nombre = nombre,
                Tipo = tipo
            });
        }
        await _db.SaveChangesAsync();
    }

    private static EmpresaResponseDto ToDto(Empresa e) => new(
        e.Id,
        e.UsuarioId,
        e.NombreEmpresa,
        e.Nit,
        e.Sector,
        e.Descripcion,
        e.FechaCreacion,
        e.TipoPersona,
        e.TipoDocumento,
        e.NumeroDocumento,
        e.DigitoVerificacion,
        e.RazonSocial,
        e.NombreComercial,
        e.PrimerNombre,
        e.SegundoNombre,
        e.PrimerApellido,
        e.SegundoApellido,
        e.Estado,
        e.Pais,
        e.Departamento,
        e.Municipio,
        e.DireccionPrincipal,
        e.ComplementoDireccion,
        e.Barrio,
        e.CodigoPostal,
        e.EmailContacto,
        e.TelefonoFijo,
        e.Celular,
        e.SitioWeb,
        e.DireccionCorrespondencia,
        e.ResponsableNombre,
        e.ResponsableTipoDocumento,
        e.ResponsableDocumento,
        e.ResponsableCargo,
        e.ResponsableRol,
        e.ResponsableEmail,
        e.ResponsableTelefono,
        e.ResponsableObservaciones,
        e.RegimenTributario,
        e.ResponsabilidadIva,
        e.CodigoCiiu,
        e.ActividadEconomica,
        e.ActividadesSecundarias,
        e.ResponsabilidadesTributarias,
        e.ObservacionesTributarias
    );
}
