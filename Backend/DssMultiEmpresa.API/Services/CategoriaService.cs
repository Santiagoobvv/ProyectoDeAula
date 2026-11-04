using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _db;

    public CategoriaService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<CategoriaResponseDto>> GetByEmpresaAsync(int empresaId)
    {
        return await _db.CategoriasContables
            .Where(c => c.EmpresaId == empresaId)
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nombre)
            .Select(c => new CategoriaResponseDto(c.Id, c.EmpresaId, c.Nombre, c.Tipo))
            .ToListAsync();
    }

    public async Task<CategoriaResponseDto> CreateAsync(int empresaId, CategoriaCreateDto dto)
    {
        var tiposValidos = new[] { "Ingreso", "Costo", "Gasto Fijo", "Gasto Variable" };
        if (!tiposValidos.Contains(dto.Tipo))
            throw new ArgumentException($"Tipo inválido: {dto.Tipo}. Use: {string.Join(", ", tiposValidos)}");

        var categoria = new CategoriaContable
        {
            EmpresaId = empresaId,
            Nombre = dto.Nombre,
            Tipo = dto.Tipo
        };

        _db.CategoriasContables.Add(categoria);
        await _db.SaveChangesAsync();

        return new CategoriaResponseDto(categoria.Id, categoria.EmpresaId, categoria.Nombre, categoria.Tipo);
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var cat = await _db.CategoriasContables
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
        if (cat is null) return false;

        _db.CategoriasContables.Remove(cat);
        await _db.SaveChangesAsync();
        return true;
    }
}
