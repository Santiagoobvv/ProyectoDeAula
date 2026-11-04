using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("cuentas_contables")]
public class CuentaContable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    /// <summary>Código contable PUC (ej: 110505, 510506, 413505...)</summary>
    [Required, MaxLength(30)]
    [Column("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    /// <summary>1=Activo, 2=Pasivo, 3=Patrimonio, 4=Ingresos, 5=Gastos, 6=Costos de Ventas, 7=Costos de Producción</summary>
    [Column("clase")]
    public int Clase { get; set; }

    /// <summary>Primeros 2 dígitos (ej: 11, 13, 22, 51...)</summary>
    [MaxLength(10)]
    [Column("grupo")]
    public string? Grupo { get; set; }

    /// <summary>Clase | Grupo | Cuenta | Subcuenta | Auxiliar</summary>
    [Required, MaxLength(30)]
    [Column("nivel")]
    public string Nivel { get; set; } = "Subcuenta";

    /// <summary>Debito | Credito</summary>
    [Required, MaxLength(10)]
    [Column("naturaleza")]
    public string Naturaleza { get; set; } = "Debito";

    /// <summary>Categoría contable PUC (Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo)</summary>
    [Required, MaxLength(50)]
    [Column("tipo_categoria")]
    public string TipoCategoria { get; set; } = "Gasto";

    /// <summary>Vínculo opcional con categoría macro DSS</summary>
    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("maneja_terceros")]
    public bool ManejaTerceros { get; set; } = false;

    /// <summary>Si es false, es una cuenta de agrupación y NO admite movimientos directos</summary>
    [Column("maneja_movimientos")]
    public bool ManejaMovimientos { get; set; } = true;

    [MaxLength(30)]
    [Column("cuenta_padre_codigo")]
    public string? CuentaPadreCodigo { get; set; }

    [Required, MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "Activa"; // "Activa" | "Inactiva"

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }

    [ForeignKey(nameof(CategoriaId))]
    public CategoriaContable? Categoria { get; set; }

    public ICollection<TransaccionContable> Transacciones { get; set; } = new List<TransaccionContable>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}
