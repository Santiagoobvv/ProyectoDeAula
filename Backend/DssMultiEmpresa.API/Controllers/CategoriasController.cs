using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
        => _categoriaService = categoriaService;

    [HttpGet]
    public async Task<IActionResult> GetAll(int empresaId)
    {
        var categorias = await _categoriaService.GetByEmpresaAsync(empresaId);
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int empresaId, [FromBody] CategoriaCreateDto dto)
    {
        try
        {
            var cat = await _categoriaService.CreateAsync(empresaId, dto);
            return Created("", cat);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int empresaId, int id)
    {
        var result = await _categoriaService.DeleteAsync(id, empresaId);
        return result ? NoContent() : NotFound();
    }
}
