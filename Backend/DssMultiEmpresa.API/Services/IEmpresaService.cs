using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface IEmpresaService
{
    Task<IEnumerable<EmpresaResponseDto>> GetByUsuarioAsync(int usuarioId);
    Task<EmpresaResponseDto?> GetByIdAsync(int id, int usuarioId);
    Task<EmpresaResponseDto> CreateAsync(int usuarioId, EmpresaCreateDto dto);
    Task<EmpresaResponseDto?> UpdateAsync(int id, int usuarioId, EmpresaUpdateDto dto);
    Task<bool> DeleteAsync(int id, int usuarioId);
    Task<bool> PerteneceAUsuarioAsync(int empresaId, int usuarioId);
}
