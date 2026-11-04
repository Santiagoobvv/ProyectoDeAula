using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/terceros")]
[Authorize]
public class TercerosController : ControllerBase
{
    private readonly ITerceroService _terceroService;

    public TercerosController(ITerceroService terceroService)
        => _terceroService = terceroService;

    [HttpGet]
    public async Task<IActionResult> GetAll(int empresaId, [FromQuery] string? busqueda, [FromQuery] string? tipoTercero)
    {
        var terceros = await _terceroService.GetByEmpresaAsync(empresaId, busqueda, tipoTercero);
        return Ok(terceros);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int empresaId, int id)
    {
        var tercero = await _terceroService.GetByIdAsync(id, empresaId);
        return tercero is null ? NotFound() : Ok(tercero);
    }

    [HttpGet("por-documento/{numeroDocumento}")]
    public async Task<IActionResult> GetByDocumento(int empresaId, string numeroDocumento)
    {
        var tercero = await _terceroService.GetByDocumentoAsync(numeroDocumento, empresaId);
        return tercero is null ? NotFound() : Ok(tercero);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int empresaId, [FromBody] TerceroCreateDto dto)
    {
        try
        {
            var result = await _terceroService.CreateAsync(empresaId, dto);
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
    public async Task<IActionResult> Update(int empresaId, int id, [FromBody] TerceroUpdateDto dto)
    {
        try
        {
            var result = await _terceroService.UpdateAsync(id, empresaId, dto);
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
        try
        {
            var result = await _terceroService.DeleteAsync(id, empresaId);
            return result ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
