using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("centros_costo")]
public class CentroCosto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    [Required, MaxLength(30)]
    [Column("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Required, MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "Activo";

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }

    public ICollection<TransaccionContable> Transacciones { get; set; } = new List<TransaccionContable>();
}
