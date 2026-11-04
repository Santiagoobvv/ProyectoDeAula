using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("terceros")]
public class Tercero
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    /// <summary>Cliente | Proveedor | Empleado | Contratista | Otro</summary>
    [Required, MaxLength(50)]
    [Column("tipo_tercero")]
    public string TipoTercero { get; set; } = "Proveedor";

    [Required, MaxLength(30)]
    [Column("tipo_documento")]
    public string TipoDocumento { get; set; } = "NIT";

    [Required, MaxLength(50)]
    [Column("numero_documento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [MaxLength(2)]
    [Column("digito_verificacion")]
    public string? DigitoVerificacion { get; set; }

    [Required, MaxLength(250)]
    [Column("razon_social")]
    public string RazonSocial { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("nombre_comercial")]
    public string? NombreComercial { get; set; }

    [MaxLength(100)]
    [Column("primer_nombre")]
    public string? PrimerNombre { get; set; }

    [MaxLength(100)]
    [Column("primer_apellido")]
    public string? PrimerApellido { get; set; }

    [MaxLength(100)]
    [Column("departamento")]
    public string? Departamento { get; set; }

    [MaxLength(100)]
    [Column("municipio")]
    public string? Municipio { get; set; }

    [MaxLength(250)]
    [Column("direccion")]
    public string? Direccion { get; set; }

    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(50)]
    [Column("telefono")]
    public string? Telefono { get; set; }

    [MaxLength(50)]
    [Column("celular")]
    public string? Celular { get; set; }

    [Required, MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "Activo";

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }

    public ICollection<TransaccionContable> Transacciones { get; set; } = new List<TransaccionContable>();
}
