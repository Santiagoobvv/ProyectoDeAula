using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

public class CuentaContableCreateDto
{
    [Required(ErrorMessage = "El código PUC es obligatorio.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "El código PUC solo debe contener números.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la cuenta es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre de la cuenta contable solo debe contener letras.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Range(1, 9, ErrorMessage = "La clase contable debe estar entre 1 y 9.")]
    public int Clase { get; set; }

    public string? Grupo { get; set; }
    public string? Nivel { get; set; }
    public string Naturaleza { get; set; } = "Debito";
    public string TipoCategoria { get; set; } = "Gasto";
    public int? CategoriaId { get; set; }
    public bool ManejaTerceros { get; set; }
    public bool ManejaMovimientos { get; set; } = true;
    public string? CuentaPadreCodigo { get; set; }
    public string? Estado { get; set; } = "Activa";
}

public class CuentaContableUpdateDto
{
    [Required(ErrorMessage = "El código PUC es obligatorio.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "El código PUC solo debe contener números.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la cuenta es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre de la cuenta contable solo debe contener letras.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Range(1, 9, ErrorMessage = "La clase contable debe estar entre 1 y 9.")]
    public int Clase { get; set; }

    public string? Grupo { get; set; }
    public string? Nivel { get; set; }
    public string Naturaleza { get; set; } = "Debito";
    public string TipoCategoria { get; set; } = "Gasto";
    public int? CategoriaId { get; set; }
    public bool ManejaTerceros { get; set; }
    public bool ManejaMovimientos { get; set; } = true;
    public string? CuentaPadreCodigo { get; set; }
    public string? Estado { get; set; } = "Activa";
}

public class CuentaContableResponseDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Clase { get; set; }
    public string? Grupo { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public string TipoCategoria { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public bool ManejaTerceros { get; set; }
    public bool ManejaMovimientos { get; set; }
    public string? CuentaPadreCodigo { get; set; }
    public string Estado { get; set; } = string.Empty;

    public CuentaContableResponseDto() { }

    public CuentaContableResponseDto(
        int id, int empresaId, string codigo, string nombre, string? descripcion, int clase, string? grupo,
        string nivel, string naturaleza, string tipoCategoria, int? categoriaId, string? categoriaNombre,
        bool manejaTerceros, bool manejaMovimientos, string? cuentaPadreCodigo, string estado)
    {
        Id = id;
        EmpresaId = empresaId;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Clase = clase;
        Grupo = grupo;
        Nivel = nivel;
        Naturaleza = naturaleza;
        TipoCategoria = tipoCategoria;
        CategoriaId = categoriaId;
        CategoriaNombre = categoriaNombre;
        ManejaTerceros = manejaTerceros;
        ManejaMovimientos = manejaMovimientos;
        CuentaPadreCodigo = cuentaPadreCodigo;
        Estado = estado;
    }
}
