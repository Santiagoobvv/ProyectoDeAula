using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

public class PresupuestoCreateDto
{
    [Range(1, 12, ErrorMessage = "El mes del periodo debe estar entre 1 y 12.")]
    public int PeriodoMes { get; set; }

    [Range(2020, 2050, ErrorMessage = "El año del periodo debe estar entre 2020 y 2050.")]
    public int PeriodoAnio { get; set; }

    [Required(ErrorMessage = "La categoría macro es obligatoria.")]
    public int CategoriaId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto presupuestado no puede ser negativo.")]
    public decimal MontoPresupuestado { get; set; }

    public string? Codigo { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre de la partida presupuestal solo debe contener letras.")]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }
    public int? CuentaContableId { get; set; }
    public string? TipoPresupuesto { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? Estado { get; set; }
    public string? Observaciones { get; set; }
}

public class PresupuestoUpdateDto
{
    [Range(1, 12, ErrorMessage = "El mes del periodo debe estar entre 1 y 12.")]
    public int PeriodoMes { get; set; }

    [Range(2020, 2050, ErrorMessage = "El año del periodo debe estar entre 2020 y 2050.")]
    public int PeriodoAnio { get; set; }

    [Required(ErrorMessage = "La categoría macro es obligatoria.")]
    public int CategoriaId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto presupuestado no puede ser negativo.")]
    public decimal MontoPresupuestado { get; set; }

    public string? Codigo { get; set; }

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El nombre de la partida presupuestal solo debe contener letras.")]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }
    public int? CuentaContableId { get; set; }
    public string? TipoPresupuesto { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? Estado { get; set; }
    public string? Observaciones { get; set; }
}

public class PresupuestoResponseDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int PeriodoMes { get; set; }
    public int PeriodoAnio { get; set; }
    public int CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public decimal MontoPresupuestado { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int? CuentaContableId { get; set; }
    public string? CuentaContableCodigo { get; set; }
    public string? CuentaContableNombre { get; set; }
    public string? TipoPresupuesto { get; set; }
    public string? CategoriaSeccion { get; set; }
    public string? Estado { get; set; }
    public string? Observaciones { get; set; }

    public PresupuestoResponseDto() { }

    public PresupuestoResponseDto(
        int id, int empresaId, int periodoMes, int periodoAnio, int categoriaId, string? categoriaNombre,
        decimal montoPresupuestado, string? codigo, string? nombre, string? descripcion, int? cuentaContableId,
        string? cuentaContableCodigo, string? cuentaContableNombre, string? tipoPresupuesto, string? categoriaSeccion,
        string? estado, string? observaciones)
    {
        Id = id;
        EmpresaId = empresaId;
        PeriodoMes = periodoMes;
        PeriodoAnio = periodoAnio;
        CategoriaId = categoriaId;
        CategoriaNombre = categoriaNombre;
        MontoPresupuestado = montoPresupuestado;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        CuentaContableId = cuentaContableId;
        CuentaContableCodigo = cuentaContableCodigo;
        CuentaContableNombre = cuentaContableNombre;
        TipoPresupuesto = tipoPresupuesto;
        CategoriaSeccion = categoriaSeccion;
        Estado = estado;
        Observaciones = observaciones;
    }
}
