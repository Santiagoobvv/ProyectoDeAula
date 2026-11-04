namespace DssMultiEmpresa.API.DTOs;

// ── DSS (Sistema de Soporte a Decisiones) ──

/// <summary>Resumen financiero global de una empresa.</summary>
public record DssResumenDto(
    int EmpresaId,
    string NombreEmpresa,
    DateTime FechaCalculo,
    decimal TotalIngresos,
    decimal TotalGastos,
    decimal FlujoCaja,
    decimal MargenOperativo,
    string NivelAlerta,
    string Recomendacion,
    List<AlertaPresupuestariaDto> AlertasPresupuestarias,
    List<EjecucionPresupuestalDto> EjecucionPresupuestal
);

/// <summary>Alerta cuando el gasto real excede un umbral del presupuesto.</summary>
public record AlertaPresupuestariaDto(
    string Categoria,
    decimal MontoPresupuestado,
    decimal MontoEjecutado,
    decimal PorcentajeEjecucion,
    string NivelAlerta,       // Verde / Amarillo / Rojo
    string Mensaje
);

/// <summary>Comparativa presupuesto vs. ejecución real por categoría.</summary>
public record EjecucionPresupuestalDto(
    int CategoriaId,
    string CategoriaNombre,
    string CategoriaTipo,
    decimal Presupuestado,
    decimal Ejecutado,
    decimal Diferencia,
    decimal PorcentajeEjecucion
);

/// <summary>Datos de flujo de caja mensual para gráficos.</summary>
public record FlujoCajaMensualDto(
    int Mes,
    int Anio,
    decimal Ingresos,
    decimal Egresos,
    decimal FlujoCaja
);
