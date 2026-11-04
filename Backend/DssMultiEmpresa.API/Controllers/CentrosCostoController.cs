using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/centros-costo")]
[Authorize]
public class CentrosCostoController : ControllerBase
{
    private readonly ICentroCostoService _centroCostoService;

    public CentrosCostoController(ICentroCostoService centroCostoService)
        => _centroCostoService = centroCostoService;

    [HttpGet]
    public async Task<IActionResult> GetAll(int empresaId)
    {
        var centros = await _centroCostoService.GetByEmpresaAsync(empresaId);
        return Ok(centros);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int empresaId, int id)
    {
        var cc = await _centroCostoService.GetByIdAsync(id, empresaId);
        return cc is null ? NotFound() : Ok(cc);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int empresaId, [FromBody] CentroCostoCreateDto dto)
    {
        try
        {
            var result = await _centroCostoService.CreateAsync(empresaId, dto);
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
    public async Task<IActionResult> Update(int empresaId, int id, [FromBody] CentroCostoUpdateDto dto)
    {
        try
        {
            var result = await _centroCostoService.UpdateAsync(id, empresaId, dto);
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
            var result = await _centroCostoService.DeleteAsync(id, empresaId);
            return result ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(int empresaId)
    {
        var count = await _centroCostoService.SeedCentrosCostoBaseAsync(empresaId);
        return Ok(new { message = $"Se cargaron {count} centros de costo base.", centrosCargados = count });
    }
}
