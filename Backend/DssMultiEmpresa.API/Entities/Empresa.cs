using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("empresas")]
public class Empresa
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Required, MaxLength(200)]
    [Column("nombre_empresa")]
    public string NombreEmpresa { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("nit")]
    public string? Nit { get; set; }

    [MaxLength(100)]
    [Column("sector")]
    public string? Sector { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // ── Identificación ampliada (Colombia) ──
    [Required, MaxLength(20)]
    [Column("tipo_persona")]
    public string TipoPersona { get; set; } = "Juridica"; // "Juridica" | "Natural"

    [Required, MaxLength(30)]
    [Column("tipo_documento")]
    public string TipoDocumento { get; set; } = "NIT"; // "NIT", "CC", "CE", "Pasaporte", "TI", "PPT", etc.

    [MaxLength(50)]
    [Column("numero_documento")]
    public string? NumeroDocumento { get; set; }

    [MaxLength(2)]
    [Column("digito_verificacion")]
    public string? DigitoVerificacion { get; set; }

    [MaxLength(250)]
    [Column("razon_social")]
    public string? RazonSocial { get; set; }

    [MaxLength(200)]
    [Column("nombre_comercial")]
    public string? NombreComercial { get; set; }

    [MaxLength(100)]
    [Column("primer_nombre")]
    public string? PrimerNombre { get; set; }

    [MaxLength(100)]
    [Column("segundo_nombre")]
    public string? SegundoNombre { get; set; }

    [MaxLength(100)]
    [Column("primer_apellido")]
    public string? PrimerApellido { get; set; }

    [MaxLength(100)]
    [Column("segundo_apellido")]
    public string? SegundoApellido { get; set; }

    [Required, MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "Activa"; // "Activa" | "Inactiva"

    // ── Ubicación y Contacto ──
    [Required, MaxLength(100)]
    [Column("pais")]
    public string Pais { get; set; } = "Colombia";

    [MaxLength(100)]
    [Column("departamento")]
    public string? Departamento { get; set; }

    [MaxLength(100)]
    [Column("municipio")]
    public string? Municipio { get; set; }

    [MaxLength(250)]
    [Column("direccion_principal")]
    public string? DireccionPrincipal { get; set; }

    [MaxLength(150)]
    [Column("complemento_direccion")]
    public string? ComplementoDireccion { get; set; }

    [MaxLength(100)]
    [Column("barrio")]
    public string? Barrio { get; set; }

    [MaxLength(20)]
    [Column("codigo_postal")]
    public string? CodigoPostal { get; set; }

    [MaxLength(255)]
    [Column("email_contacto")]
    public string? EmailContacto { get; set; }

    [MaxLength(50)]
    [Column("telefono_fijo")]
    public string? TelefonoFijo { get; set; }

    [MaxLength(50)]
    [Column("celular")]
    public string? Celular { get; set; }

    [MaxLength(255)]
    [Column("sitio_web")]
    public string? SitioWeb { get; set; }

    [MaxLength(250)]
    [Column("direccion_correspondencia")]
    public string? DireccionCorrespondencia { get; set; }

    // ── Responsable de la Empresa ──
    [MaxLength(200)]
    [Column("responsable_nombre")]
    public string? ResponsableNombre { get; set; }

    [MaxLength(30)]
    [Column("responsable_tipo_documento")]
    public string? ResponsableTipoDocumento { get; set; }

    [MaxLength(50)]
    [Column("responsable_documento")]
    public string? ResponsableDocumento { get; set; }

    [MaxLength(100)]
    [Column("responsable_cargo")]
    public string? ResponsableCargo { get; set; }

    [MaxLength(50)]
    [Column("responsable_rol")]
    public string? ResponsableRol { get; set; } // "Representante Legal", "Contacto Administrativo", "Responsable Financiero", "Contador Encargado"

    [MaxLength(255)]
    [Column("responsable_email")]
    public string? ResponsableEmail { get; set; }

    [MaxLength(50)]
    [Column("responsable_telefono")]
    public string? ResponsableTelefono { get; set; }

    [Column("responsable_observaciones")]
    public string? ResponsableObservaciones { get; set; }

    // ── Información Tributaria Colombia ──
    [MaxLength(100)]
    [Column("regimen_tributario")]
    public string? RegimenTributario { get; set; } // "Responsable de IVA", "No responsable de IVA", "Régimen Simple de Tributación (RST)", "Gran Contribuyente"

    [MaxLength(50)]
    [Column("responsabilidad_iva")]
    public string? ResponsabilidadIva { get; set; }

    [MaxLength(10)]
    [Column("codigo_ciiu")]
    public string? CodigoCiiu { get; set; }

    [MaxLength(250)]
    [Column("actividad_economica")]
    public string? ActividadEconomica { get; set; }

    [MaxLength(250)]
    [Column("actividades_secundarias")]
    public string? ActividadesSecundarias { get; set; }

    [Column("responsabilidades_tributarias")]
    public string? ResponsabilidadesTributarias { get; set; }

    [Column("observaciones_tributarias")]
    public string? ObservacionesTributarias { get; set; }

    // Navegación
    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    public ICollection<CategoriaContable> Categorias { get; set; } = new List<CategoriaContable>();
    public ICollection<CuentaContable> CuentasContables { get; set; } = new List<CuentaContable>();
    public ICollection<Tercero> Terceros { get; set; } = new List<Tercero>();
    public ICollection<CentroCosto> CentrosCosto { get; set; } = new List<CentroCosto>();
    public ICollection<TransaccionContable> Transacciones { get; set; } = new List<TransaccionContable>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    public ICollection<IndicadorDss> Indicadores { get; set; } = new List<IndicadorDss>();
}
