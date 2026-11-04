using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

public class EmpresaCreateDto
{
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? Nit { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El sector económico solo debe contener letras.")]
    public string? Sector { get; set; }
    public string? Descripcion { get; set; }

    // Identificación
    public string? TipoPersona { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }

    [RegularExpression(@"^[0-9]?$", ErrorMessage = "El dígito de verificación debe ser un número de un dígito.")]
    public string? DigitoVerificacion { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "La razón social solo debe contener letras.")]
    public string? RazonSocial { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre comercial solo debe contener letras.")]
    public string? NombreComercial { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer nombre solo debe contener letras.")]
    public string? PrimerNombre { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El segundo nombre solo debe contener letras.")]
    public string? SegundoNombre { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer apellido solo debe contener letras.")]
    public string? PrimerApellido { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El segundo apellido solo debe contener letras.")]
    public string? SegundoApellido { get; set; }

    public string? Estado { get; set; }

    // Ubicación y Contacto
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El país solo debe contener letras.")]
    public string? Pais { get; set; }
    public string? Departamento { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El municipio solo debe contener letras.")]
    public string? Municipio { get; set; }
    public string? DireccionPrincipal { get; set; }
    public string? ComplementoDireccion { get; set; }
    public string? Barrio { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El código postal solo debe contener números.")]
    public string? CodigoPostal { get; set; }
    public string? EmailContacto { get; set; }
    public string? TelefonoFijo { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El número celular solo debe contener números.")]
    public string? Celular { get; set; }
    public string? SitioWeb { get; set; }
    public string? DireccionCorrespondencia { get; set; }

    // Responsable
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre del responsable solo debe contener letras.")]
    public string? ResponsableNombre { get; set; }
    public string? ResponsableTipoDocumento { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El documento del responsable solo debe contener números.")]
    public string? ResponsableDocumento { get; set; }
    public string? ResponsableCargo { get; set; }
    public string? ResponsableRol { get; set; }
    public string? ResponsableEmail { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El teléfono del responsable solo debe contener números.")]
    public string? ResponsableTelefono { get; set; }
    public string? ResponsableObservaciones { get; set; }

    // Tributaria
    public string? RegimenTributario { get; set; }
    public string? ResponsabilidadIva { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El código CIIU principal solo debe contener números.")]
    public string? CodigoCiiu { get; set; }
    public string? ActividadEconomica { get; set; }

    [RegularExpression(@"^[0-9,\s]*$", ErrorMessage = "Las actividades secundarias solo deben contener códigos numéricos separados por comas.")]
    public string? ActividadesSecundarias { get; set; }
    public string? ResponsabilidadesTributarias { get; set; }
    public string? ObservacionesTributarias { get; set; }
}

public class EmpresaUpdateDto
{
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? Nit { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El sector económico solo debe contener letras.")]
    public string? Sector { get; set; }
    public string? Descripcion { get; set; }

    // Identificación
    public string? TipoPersona { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }

    [RegularExpression(@"^[0-9]?$", ErrorMessage = "El dígito de verificación debe ser un número de un dígito.")]
    public string? DigitoVerificacion { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "La razón social solo debe contener letras.")]
    public string? RazonSocial { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre comercial solo debe contener letras.")]
    public string? NombreComercial { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer nombre solo debe contener letras.")]
    public string? PrimerNombre { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El segundo nombre solo debe contener letras.")]
    public string? SegundoNombre { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer apellido solo debe contener letras.")]
    public string? PrimerApellido { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El segundo apellido solo debe contener letras.")]
    public string? SegundoApellido { get; set; }

    public string? Estado { get; set; }

    // Ubicación y Contacto
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El país solo debe contener letras.")]
    public string? Pais { get; set; }
    public string? Departamento { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El municipio solo debe contener letras.")]
    public string? Municipio { get; set; }
    public string? DireccionPrincipal { get; set; }
    public string? ComplementoDireccion { get; set; }
    public string? Barrio { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El código postal solo debe contener números.")]
    public string? CodigoPostal { get; set; }
    public string? EmailContacto { get; set; }
    public string? TelefonoFijo { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El número celular solo debe contener números.")]
    public string? Celular { get; set; }
    public string? SitioWeb { get; set; }
    public string? DireccionCorrespondencia { get; set; }

    // Responsable
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre del responsable solo debe contener letras.")]
    public string? ResponsableNombre { get; set; }
    public string? ResponsableTipoDocumento { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El documento del responsable solo debe contener números.")]
    public string? ResponsableDocumento { get; set; }
    public string? ResponsableCargo { get; set; }
    public string? ResponsableRol { get; set; }
    public string? ResponsableEmail { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El teléfono del responsable solo debe contener números.")]
    public string? ResponsableTelefono { get; set; }
    public string? ResponsableObservaciones { get; set; }

    // Tributaria
    public string? RegimenTributario { get; set; }
    public string? ResponsabilidadIva { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El código CIIU principal solo debe contener números.")]
    public string? CodigoCiiu { get; set; }
    public string? ActividadEconomica { get; set; }

    [RegularExpression(@"^[0-9,\s]*$", ErrorMessage = "Las actividades secundarias solo deben contener códigos numéricos separados por comas.")]
    public string? ActividadesSecundarias { get; set; }
    public string? ResponsabilidadesTributarias { get; set; }
    public string? ObservacionesTributarias { get; set; }
}

public class EmpresaResponseDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string? Sector { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }

    public string TipoPersona { get; set; } = "Juridica";
    public string TipoDocumento { get; set; } = "NIT";
    public string? NumeroDocumento { get; set; }
    public string? DigitoVerificacion { get; set; }
    public string? RazonSocial { get; set; }
    public string? NombreComercial { get; set; }
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string Estado { get; set; } = "Activa";

    public string Pais { get; set; } = "Colombia";
    public string? Departamento { get; set; }
    public string? Municipio { get; set; }
    public string? DireccionPrincipal { get; set; }
    public string? ComplementoDireccion { get; set; }
    public string? Barrio { get; set; }
    public string? CodigoPostal { get; set; }
    public string? EmailContacto { get; set; }
    public string? TelefonoFijo { get; set; }
    public string? Celular { get; set; }
    public string? SitioWeb { get; set; }
    public string? DireccionCorrespondencia { get; set; }

    public string? ResponsableNombre { get; set; }
    public string? ResponsableTipoDocumento { get; set; }
    public string? ResponsableDocumento { get; set; }
    public string? ResponsableCargo { get; set; }
    public string? ResponsableRol { get; set; }
    public string? ResponsableEmail { get; set; }
    public string? ResponsableTelefono { get; set; }
    public string? ResponsableObservaciones { get; set; }

    public string? RegimenTributario { get; set; }
    public string? ResponsabilidadIva { get; set; }
    public string? CodigoCiiu { get; set; }
    public string? ActividadEconomica { get; set; }
    public string? ActividadesSecundarias { get; set; }
    public string? ResponsabilidadesTributarias { get; set; }
    public string? ObservacionesTributarias { get; set; }

    public EmpresaResponseDto() { }

    public EmpresaResponseDto(
        int id, int usuarioId, string nombreEmpresa, string? nit, string? sector, string? descripcion, DateTime fechaCreacion,
        string tipoPersona, string tipoDocumento, string? numeroDocumento, string? digitoVerificacion, string? razonSocial,
        string? nombreComercial, string? primerNombre, string? segundoNombre, string? primerApellido, string? segundoApellido,
        string estado, string pais, string? departamento, string? municipio, string? direccionPrincipal, string? complementoDireccion,
        string? barrio, string? codigoPostal, string? emailContacto, string? telefonoFijo, string? celular, string? sitioWeb,
        string? direccionCorrespondencia, string? responsableNombre, string? responsableTipoDocumento, string? responsableDocumento,
        string? responsableCargo, string? responsableRol, string? responsableEmail, string? responsableTelefono, string? responsableObservaciones,
        string? regimenTributario, string? responsabilidadIva, string? codigoCiiu, string? actividadEconomica, string? actividadesSecundarias,
        string? responsabilidadesTributarias, string? observacionesTributarias)
    {
        Id = id;
        UsuarioId = usuarioId;
        NombreEmpresa = nombreEmpresa;
        Nit = nit;
        Sector = sector;
        Descripcion = descripcion;
        FechaCreacion = fechaCreacion;
        TipoPersona = tipoPersona;
        TipoDocumento = tipoDocumento;
        NumeroDocumento = numeroDocumento;
        DigitoVerificacion = digitoVerificacion;
        RazonSocial = razonSocial;
        NombreComercial = nombreComercial;
        PrimerNombre = primerNombre;
        SegundoNombre = segundoNombre;
        PrimerApellido = primerApellido;
        SegundoApellido = segundoApellido;
        Estado = estado;
        Pais = pais;
        Departamento = departamento;
        Municipio = municipio;
        DireccionPrincipal = direccionPrincipal;
        ComplementoDireccion = complementoDireccion;
        Barrio = barrio;
        CodigoPostal = codigoPostal;
        EmailContacto = emailContacto;
        TelefonoFijo = telefonoFijo;
        Celular = celular;
        SitioWeb = sitioWeb;
        DireccionCorrespondencia = direccionCorrespondencia;
        ResponsableNombre = responsableNombre;
        ResponsableTipoDocumento = responsableTipoDocumento;
        ResponsableDocumento = responsableDocumento;
        ResponsableCargo = responsableCargo;
        ResponsableRol = responsableRol;
        ResponsableEmail = responsableEmail;
        ResponsableTelefono = responsableTelefono;
        ResponsableObservaciones = responsableObservaciones;
        RegimenTributario = regimenTributario;
        ResponsabilidadIva = responsabilidadIva;
        CodigoCiiu = codigoCiiu;
        ActividadEconomica = actividadEconomica;
        ActividadesSecundarias = actividadesSecundarias;
        ResponsabilidadesTributarias = responsabilidadesTributarias;
        ObservacionesTributarias = observacionesTributarias;
    }
}
