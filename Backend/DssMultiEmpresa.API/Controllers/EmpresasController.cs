using System.Security.Claims;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DssMultiEmpresa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresasController(IEmpresaService empresaService) => _empresaService = empresaService;

    private int GetUsuarioId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Obtener todas las empresas del usuario autenticado.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var empresas = await _empresaService.GetByUsuarioAsync(GetUsuarioId());
        return Ok(empresas);
    }

    /// <summary>Obtener una empresa por ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empresa = await _empresaService.GetByIdAsync(id, GetUsuarioId());
        return empresa is null ? NotFound() : Ok(empresa);
    }

    /// <summary>Crear una nueva empresa/cliente.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpresaCreateDto dto)
    {
        var empresa = await _empresaService.CreateAsync(GetUsuarioId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, empresa);
    }

    /// <summary>Actualizar una empresa existente.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmpresaUpdateDto dto)
    {
        var empresa = await _empresaService.UpdateAsync(id, GetUsuarioId(), dto);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    /// <summary>Eliminar una empresa.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _empresaService.DeleteAsync(id, GetUsuarioId());
        return result ? NoContent() : NotFound();
    }
}
