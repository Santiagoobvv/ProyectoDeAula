using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/dss")]
[Authorize]
public class DssController : ControllerBase
{
    private readonly IDssService _dssService;

    public DssController(IDssService dssService) => _dssService = dssService;

    /// <summary>Obtener resumen DSS completo (KPIs, alertas, recomendaciones) de una empresa en un periodo.</summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(int empresaId, [FromQuery] int mes, [FromQuery] int anio)
    {
        try
        {
            var resumen = await _dssService.CalcularResumenAsync(empresaId, mes, anio);
            return Ok(resumen);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>Obtener flujo de caja mensual (últimos N meses) para gráficos.</summary>
    [HttpGet("flujo-caja")]
    public async Task<IActionResult> GetFlujoCaja(int empresaId, [FromQuery] int meses = 12)
    {
        var flujo = await _dssService.ObtenerFlujoCajaMensualAsync(empresaId, meses);
        return Ok(flujo);
    }
}
