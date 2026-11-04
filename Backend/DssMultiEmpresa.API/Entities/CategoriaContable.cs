using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("categorias_contables")]
public class CategoriaContable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    [Required, MaxLength(150)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Ingreso | Costo | Gasto Fijo | Gasto Variable</summary>
    [Required, MaxLength(30)]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }

    public ICollection<TransaccionContable> Transacciones { get; set; } = new List<TransaccionContable>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}
