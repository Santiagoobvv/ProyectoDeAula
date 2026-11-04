using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

public class CentroCostoCreateDto
{
    [Required(ErrorMessage = "El código del centro de costo es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del centro de costo es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre del centro de costo solo debe contener letras.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "Activo";
}

public class CentroCostoUpdateDto
{
    [Required(ErrorMessage = "El código del centro de costo es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del centro de costo es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre del centro de costo solo debe contener letras.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "Activo";
}

public class CentroCostoResponseDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = string.Empty;

    public CentroCostoResponseDto() { }

    public CentroCostoResponseDto(int id, int empresaId, string codigo, string nombre, string? descripcion, string estado)
    {
        Id = id;
        EmpresaId = empresaId;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Estado = estado;
    }
}
