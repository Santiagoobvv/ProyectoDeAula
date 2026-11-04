using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaResponseDto>> GetByEmpresaAsync(int empresaId);
    Task<CategoriaResponseDto> CreateAsync(int empresaId, CategoriaCreateDto dto);
    Task<bool> DeleteAsync(int id, int empresaId);
}
