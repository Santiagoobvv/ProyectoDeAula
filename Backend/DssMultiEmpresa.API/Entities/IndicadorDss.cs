using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("indicadores_dss")]
public class IndicadorDss
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    [Column("fecha_calculo")]
    public DateTime FechaCalculo { get; set; } = DateTime.UtcNow;

    [Column("flujo_caja")]
    public decimal FlujoCaja { get; set; }

    [Column("margen_operativo")]
    public decimal MargenOperativo { get; set; }

    [Column("total_ingresos")]
    public decimal TotalIngresos { get; set; }

    [Column("total_gastos")]
    public decimal TotalGastos { get; set; }

    /// <summary>Verde | Amarillo | Rojo</summary>
    [Required, MaxLength(10)]
    [Column("nivel_alerta")]
    public string NivelAlerta { get; set; } = "Verde";

    [Column("recomendacion")]
    public string? Recomendacion { get; set; }

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }
}
