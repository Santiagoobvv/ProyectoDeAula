using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/cuentas")]
[Authorize]
public class CuentasContablesController : ControllerBase
{
    private readonly ICuentaContableService _cuentaService;

    public CuentasContablesController(ICuentaContableService cuentaService)
        => _cuentaService = cuentaService;

    /// <summary>Listar cuentas contables de una empresa con filtros avanzados.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int empresaId,
        [FromQuery] string? busqueda,
        [FromQuery] int? clase,
        [FromQuery] string? tipoCategoria,
        [FromQuery] string? naturaleza,
        [FromQuery] string? estado)
    {
        var cuentas = await _cuentaService.GetByEmpresaAsync(
            empresaId, busqueda, clase, tipoCategoria, naturaleza, estado);
        return Ok(cuentas);
    }

    /// <summary>Obtener una cuenta contable por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int empresaId, int id)
    {
        var cuenta = await _cuentaService.GetByIdAsync(id, empresaId);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    /// <summary>Buscar una cuenta contable por código PUC.</summary>
    [HttpGet("por-codigo/{codigo}")]
    public async Task<IActionResult> GetByCodigo(int empresaId, string codigo)
    {
        var cuenta = await _cuentaService.GetByCodigoAsync(codigo, empresaId);
        return cuenta is null ? NotFound(new { error = $"No se encontró la cuenta con código '{codigo}'" }) : Ok(cuenta);
    }

    /// <summary>Crear una nueva cuenta contable.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(int empresaId, [FromBody] CuentaContableCreateDto dto)
    {
        try
        {
            var result = await _cuentaService.CreateAsync(empresaId, dto);
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

    /// <summary>Actualizar una cuenta contable existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int empresaId, int id, [FromBody] CuentaContableUpdateDto dto)
    {
        try
        {
            var result = await _cuentaService.UpdateAsync(id, empresaId, dto);
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

    /// <summary>Alternar estado activo/inactivo de una cuenta contable.</summary>
    [HttpPatch("{id:int}/toggle-estado")]
    public async Task<IActionResult> ToggleEstado(int empresaId, int id)
    {
        var success = await _cuentaService.ToggleEstadoAsync(id, empresaId);
        return success ? Ok(new { message = "Estado actualizado" }) : NotFound();
    }

    /// <summary>Eliminar una cuenta contable (si no tiene movimientos).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int empresaId, int id)
    {
        try
        {
            var result = await _cuentaService.DeleteAsync(id, empresaId);
            return result ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Sembrar catálogo inicial de cuentas contables PUC colombiano.</summary>
    [HttpPost("seed-puc")]
    public async Task<IActionResult> SeedPuc(int empresaId)
    {
        var count = await _cuentaService.SeedPucBaseAsync(empresaId);
        return Ok(new { message = $"Se cargaron {count} cuentas contables del PUC colombiano con éxito.", cuentasCargadas = count });
    }
}
