using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DssMultiEmpresa.API.Entities;

[Table("transacciones_contables")]
public class TransaccionContable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    [Column("fecha")]
    public DateOnly Fecha { get; set; }

    [Required, MaxLength(300)]
    [Column("concepto")]
    public string Concepto { get; set; } = string.Empty;

    [Column("monto")]
    public decimal Monto { get; set; }

    /// <summary>Ingreso | Egreso (para compatibilidad con DSS)</summary>
    [Required, MaxLength(20)]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    // ── Campos contables ampliados ──
    [MaxLength(50)]
    [Column("numero_transaccion")]
    public string? NumeroTransaccion { get; set; }

    [Column("cuenta_contable_id")]
    public int? CuentaContableId { get; set; }

    [MaxLength(30)]
    [Column("codigo_cuenta")]
    public string? CodigoCuenta { get; set; }

    [MaxLength(200)]
    [Column("nombre_cuenta")]
    public string? NombreCuenta { get; set; }

    [Column("tercero_id")]
    public int? TerceroId { get; set; }

    [MaxLength(30)]
    [Column("tercero_tipo_doc")]
    public string? TerceroTipoDoc { get; set; }

    [MaxLength(50)]
    [Column("tercero_numero_doc")]
    public string? TerceroNumeroDoc { get; set; }

    [MaxLength(2)]
    [Column("tercero_digito_verificacion")]
    public string? TerceroDigitoVerificacion { get; set; }

    [MaxLength(50)]
    [Column("tercero_nit")]
    public string? TerceroNit { get; set; }

    [MaxLength(200)]
    [Column("tercero_nombre")]
    public string? TerceroNombre { get; set; }

    [Column("centro_costo_id")]
    public int? CentroCostoId { get; set; }

    [MaxLength(100)]
    [Column("centro_costo_nombre")]
    public string? CentroCostoNombre { get; set; }

    /// <summary>Debito | Credito</summary>
    [MaxLength(10)]
    [Column("naturaleza_movimiento")]
    public string NaturalezaMovimiento { get; set; } = "Debito";

    [Column("monto_debito")]
    public decimal MontoDebito { get; set; } = 0;

    [Column("monto_credito")]
    public decimal MontoCredito { get; set; } = 0;

    [MaxLength(50)]
    [Column("tipo_documento_soporte")]
    public string? TipoDocumentoSoporte { get; set; }

    [MaxLength(50)]
    [Column("numero_documento_soporte")]
    public string? NumeroDocumentoSoporte { get; set; }

    [Column("fecha_documento_soporte")]
    public DateOnly? FechaDocumentoSoporte { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    // Navegación
    [ForeignKey(nameof(EmpresaId))]
    public Empresa? Empresa { get; set; }

    [ForeignKey(nameof(CategoriaId))]
    public CategoriaContable? Categoria { get; set; }

    [ForeignKey(nameof(CuentaContableId))]
    public CuentaContable? CuentaContable { get; set; }

    [ForeignKey(nameof(TerceroId))]
    public Tercero? Tercero { get; set; }

    [ForeignKey(nameof(CentroCostoId))]
    public CentroCosto? CentroCosto { get; set; }
}
