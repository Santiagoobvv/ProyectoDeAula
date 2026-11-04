using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface IDssService
{
    /// <summary>Calcula el resumen financiero DSS completo para una empresa en un periodo dado.</summary>
    Task<DssResumenDto> CalcularResumenAsync(int empresaId, int mes, int anio);

    /// <summary>Obtiene el flujo de caja mensual de los últimos N meses para gráficos.</summary>
    Task<IEnumerable<FlujoCajaMensualDto>> ObtenerFlujoCajaMensualAsync(int empresaId, int cantidadMeses = 12);
}
