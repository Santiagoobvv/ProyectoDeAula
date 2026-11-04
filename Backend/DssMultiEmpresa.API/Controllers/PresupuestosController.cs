using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/presupuestos")]
[Authorize]
public class PresupuestosController : ControllerBase
{
    private readonly IPresupuestoService _presupuestoService;

    public PresupuestosController(IPresupuestoService presupuestoService)
        => _presupuestoService = presupuestoService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int empresaId,
        [FromQuery] int? mes,
        [FromQuery] int? anio,
        [FromQuery] string? busqueda,
        [FromQuery] string? categoria,
        [FromQuery] string? tipoPresupuesto,
        [FromQuery] string? estado)
    {
        var presupuestos = await _presupuestoService.GetByEmpresaAsync(
            empresaId, mes, anio, busqueda, categoria, tipoPresupuesto, estado);
        return Ok(presupuestos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int empresaId, int id)
    {
        var p = await _presupuestoService.GetByIdAsync(id, empresaId);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int empresaId, [FromBody] PresupuestoCreateDto dto)
    {
        try
        {
            var result = await _presupuestoService.CreateAsync(empresaId, dto);
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int empresaId, int id, [FromBody] PresupuestoUpdateDto dto)
    {
        try
        {
            var result = await _presupuestoService.UpdateAsync(id, empresaId, dto);
            return result is null ? NotFound() : Ok(result);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int empresaId, int id)
    {
        var result = await _presupuestoService.DeleteAsync(id, empresaId);
        return result ? NoContent() : NotFound();
    }
}
