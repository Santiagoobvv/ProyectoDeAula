using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("presupuestos")]
public class Presupuesto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    [Column("periodo_mes")]
    [Range(1, 12)]
    public int PeriodoMes { get; set; }

    [Column("periodo_anio")]
    public int PeriodoAnio { get; set; }

    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [Column("monto_presupuestado")]
    public decimal MontoPresupuestado { get; set; }

    // ── Campos ampliados ──
    [MaxLength(50)]
    [Column("codigo")]
    public string? Codigo { get; set; }

    [MaxLength(200)]
    [Column("nombre")]
    public string? Nombre { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("cuenta_contable_id")]
    public int? CuentaContableId { get; set; }

    /// <summary>Ingreso | Gasto Fijo | Gasto Variable | Costo | Inversion</summary>
    [MaxLength(50)]
    [Column("tipo_presupuesto")]
    public string TipoPresupuesto { get; set; } = "Gasto Fijo";

    /// <summary>Honorarios | Servicios | Personal | Educacion | Operacion | Ingresos | Otros</summary>
    [MaxLength(100)]
    [Column("categoria_nombre")]
    public string? CategoriaNombre { get; set; }

    [MaxLength(30)]
    [Column("estado")]
    public string Estado { get; set; } = "Activo"; // "Planificado" | "Aprobado" | "En Ejecucion" | "Cerrado" | "Activo"

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }

    [ForeignKey(nameof(CategoriaId))]
    public CategoriaContable? Categoria { get; set; }

    [ForeignKey(nameof(CuentaContableId))]
    public CuentaContable? CuentaContable { get; set; }
}
