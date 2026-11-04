using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

public class TransaccionCreateDto
{
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "El concepto de la transacción es obligatorio.")]
    [StringLength(300, ErrorMessage = "El concepto no puede superar los 300 caracteres.")]
    public string Concepto { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    [Required(ErrorMessage = "El tipo de transacción es obligatorio.")]
    [RegularExpression(@"^(Ingreso|Egreso)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Egreso'.")]
    public string Tipo { get; set; } = "Egreso";

    public int? CategoriaId { get; set; }
    public string? Observaciones { get; set; }

    public string? NumeroTransaccion { get; set; }
    public int? CuentaContableId { get; set; }
    public string? CodigoCuenta { get; set; }
    public string? NombreCuenta { get; set; }

    public int? TerceroId { get; set; }
    public string? TerceroTipoDoc { get; set; }
    public string? TerceroNumeroDoc { get; set; }
    public string? TerceroDigitoVerificacion { get; set; }
    public string? TerceroNit { get; set; }
    public string? TerceroNombre { get; set; }

    public int? CentroCostoId { get; set; }
    public string? CentroCostoNombre { get; set; }

    [RegularExpression(@"^(Debito|Credito)$", ErrorMessage = "La naturaleza del movimiento debe ser 'Debito' o 'Credito'.")]
    public string? NaturalezaMovimiento { get; set; }

    public decimal? MontoDebito { get; set; }
    public decimal? MontoCredito { get; set; }
    public string? TipoDocumentoSoporte { get; set; }
    public string? NumeroDocumentoSoporte { get; set; }
    public DateOnly? FechaDocumentoSoporte { get; set; }
    public string? Descripcion { get; set; }
}

public class TransaccionResponseDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? Observaciones { get; set; }

    public string? NumeroTransaccion { get; set; }
    public int? CuentaContableId { get; set; }
    public string? CodigoCuenta { get; set; }
    public string? NombreCuenta { get; set; }

    public int? TerceroId { get; set; }
    public string? TerceroTipoDoc { get; set; }
    public string? TerceroNumeroDoc { get; set; }
    public string? TerceroDigitoVerificacion { get; set; }
    public string? TerceroNit { get; set; }
    public string? TerceroNombre { get; set; }

    public int? CentroCostoId { get; set; }
    public string? CentroCostoNombre { get; set; }
    public string NaturalezaMovimiento { get; set; } = "Debito";
    public decimal MontoDebito { get; set; }
    public decimal MontoCredito { get; set; }
    public string? TipoDocumentoSoporte { get; set; }
    public string? NumeroDocumentoSoporte { get; set; }
    public DateOnly? FechaDocumentoSoporte { get; set; }
    public string? Descripcion { get; set; }

    public TransaccionResponseDto() { }

    public TransaccionResponseDto(
        int id, int empresaId, DateOnly fecha, string concepto, decimal monto, string tipo, int? categoriaId,
        string? categoriaNombre, string? observaciones, string? numeroTransaccion, int? cuentaContableId,
        string? codigoCuenta, string? nombreCuenta, int? terceroId, string? terceroNit, string? terceroNombre,
        int? centroCostoId, string? centroCostoNombre, string naturalezaMovimiento, decimal montoDebito,
        decimal montoCredito, string? tipoDocumentoSoporte, string? numeroDocumentoSoporte,
        DateOnly? fechaDocumentoSoporte, string? descripcion, string? terceroTipoDoc = null,
        string? terceroNumeroDoc = null, string? terceroDigitoVerificacion = null)
    {
        Id = id;
        EmpresaId = empresaId;
        Fecha = fecha;
        Concepto = concepto;
        Monto = monto;
        Tipo = tipo;
        CategoriaId = categoriaId;
        CategoriaNombre = categoriaNombre;
        Observaciones = observaciones;
        NumeroTransaccion = numeroTransaccion;
        CuentaContableId = cuentaContableId;
        CodigoCuenta = codigoCuenta;
        NombreCuenta = nombreCuenta;
        TerceroId = terceroId;
        TerceroNit = terceroNit;
        TerceroNombre = terceroNombre;
        CentroCostoId = centroCostoId;
        CentroCostoNombre = centroCostoNombre;
        NaturalezaMovimiento = naturalezaMovimiento;
        MontoDebito = montoDebito;
        MontoCredito = montoCredito;
        TipoDocumentoSoporte = tipoDocumentoSoporte;
        NumeroDocumentoSoporte = numeroDocumentoSoporte;
        FechaDocumentoSoporte = fechaDocumentoSoporte;
        Descripcion = descripcion;
        TerceroTipoDoc = terceroTipoDoc;
        TerceroNumeroDoc = terceroNumeroDoc ?? terceroNit;
        TerceroDigitoVerificacion = terceroDigitoVerificacion;
    }
}
