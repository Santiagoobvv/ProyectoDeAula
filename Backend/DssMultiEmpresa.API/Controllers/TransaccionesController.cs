using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/transacciones")]
[Authorize]
public class TransaccionesController : ControllerBase
{
    private readonly ITransaccionService _transaccionService;

    public TransaccionesController(ITransaccionService transaccionService)
        => _transaccionService = transaccionService;

    /// <summary>Listar transacciones con filtros contables avanzados.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int empresaId,
        [FromQuery] int? mes,
        [FromQuery] int? anio,
        [FromQuery] string? busqueda,
        [FromQuery] int? cuentaContableId,
        [FromQuery] string? codigoCuenta,
        [FromQuery] int? terceroId,
        [FromQuery] int? centroCostoId,
        [FromQuery] string? tipo)
    {
        var transacciones = await _transaccionService.GetByEmpresaAsync(
            empresaId, mes, anio, busqueda, cuentaContableId, codigoCuenta, terceroId, centroCostoId, tipo);
        return Ok(transacciones);
    }

    /// <summary>Obtener detalle de una transacción por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int empresaId, int id)
    {
        var transaccion = await _transaccionService.GetByIdAsync(id, empresaId);
        return transaccion is null ? NotFound() : Ok(transaccion);
    }

    /// <summary>Registrar una nueva transacción contable con cuenta PUC, tercero y centro de costo.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(int empresaId, [FromBody] TransaccionCreateDto dto)
    {
        try
        {
            var result = await _transaccionService.CreateAsync(empresaId, dto);
            return CreatedAtAction(nameof(GetById), new { empresaId, id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Eliminar una transacción contable.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int empresaId, int id)
    {
        var result = await _transaccionService.DeleteAsync(id, empresaId);
        return result ? NoContent() : NotFound();
    }
}
