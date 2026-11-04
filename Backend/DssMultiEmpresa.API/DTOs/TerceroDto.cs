using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

public class TerceroCreateDto
{
    [Required(ErrorMessage = "El tipo de tercero es obligatorio.")]
    public string TipoTercero { get; set; } = "Proveedor";

    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public string TipoDocumento { get; set; } = "NIT";

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]?$", ErrorMessage = "El dígito de verificación debe ser un número de un dígito.")]
    public string? DigitoVerificacion { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "La razón social solo debe contener letras.")]
    public string RazonSocial { get; set; } = string.Empty;

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre comercial solo debe contener letras.")]
    public string? NombreComercial { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer nombre solo debe contener letras.")]
    public string? PrimerNombre { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer apellido solo debe contener letras.")]
    public string? PrimerApellido { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El departamento solo debe contener letras.")]
    public string? Departamento { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El municipio solo debe contener letras.")]
    public string? Municipio { get; set; }

    public string? Direccion { get; set; }

    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido.")]
    public string? Email { get; set; }

    public string? Telefono { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El teléfono celular solo debe contener números.")]
    public string? Celular { get; set; }

    public string Estado { get; set; } = "Activo";
    public string? Observaciones { get; set; }
}

public class TerceroUpdateDto
{
    [Required(ErrorMessage = "El tipo de tercero es obligatorio.")]
    public string TipoTercero { get; set; } = "Proveedor";

    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public string TipoDocumento { get; set; } = "NIT";

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]?$", ErrorMessage = "El dígito de verificación debe ser un número de un dígito.")]
    public string? DigitoVerificacion { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "La razón social solo debe contener letras.")]
    public string RazonSocial { get; set; } = string.Empty;

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre comercial solo debe contener letras.")]
    public string? NombreComercial { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer nombre solo debe contener letras.")]
    public string? PrimerNombre { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El primer apellido solo debe contener letras.")]
    public string? PrimerApellido { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El departamento solo debe contener letras.")]
    public string? Departamento { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El municipio solo debe contener letras.")]
    public string? Municipio { get; set; }

    public string? Direccion { get; set; }

    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido.")]
    public string? Email { get; set; }

    public string? Telefono { get; set; }

    [RegularExpression(@"^[0-9]*$", ErrorMessage = "El teléfono celular solo debe contener números.")]
    public string? Celular { get; set; }

    public string Estado { get; set; } = "Activo";
    public string? Observaciones { get; set; }
}

public class TerceroResponseDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string TipoTercero { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? DigitoVerificacion { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? PrimerNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? Departamento { get; set; }
    public string? Municipio { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    public TerceroResponseDto() { }

    public TerceroResponseDto(
        int id, int empresaId, string tipoTercero, string tipoDocumento, string numeroDocumento,
        string? digitoVerificacion, string razonSocial, string? nombreComercial, string? primerNombre,
        string? primerApellido, string? departamento, string? municipio, string? direccion, string? email,
        string? telefono, string? celular, string estado, string? observaciones)
    {
        Id = id;
        EmpresaId = empresaId;
        TipoTercero = tipoTercero;
        TipoDocumento = tipoDocumento;
        NumeroDocumento = numeroDocumento;
        DigitoVerificacion = digitoVerificacion;
        RazonSocial = razonSocial;
        NombreComercial = nombreComercial;
        PrimerNombre = primerNombre;
        PrimerApellido = primerApellido;
        Departamento = departamento;
        Municipio = municipio;
        Direccion = direccion;
        Email = email;
        Telefono = telefono;
        Celular = celular;
        Estado = estado;
        Observaciones = observaciones;
    }
}
