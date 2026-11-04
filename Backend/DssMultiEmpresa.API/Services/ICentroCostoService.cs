using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface ICentroCostoService
{
    Task<IEnumerable<CentroCostoResponseDto>> GetByEmpresaAsync(int empresaId);
    Task<CentroCostoResponseDto?> GetByIdAsync(int id, int empresaId);
    Task<CentroCostoResponseDto> CreateAsync(int empresaId, CentroCostoCreateDto dto);
    Task<CentroCostoResponseDto?> UpdateAsync(int id, int empresaId, CentroCostoUpdateDto dto);
    Task<bool> DeleteAsync(int id, int empresaId);
    Task<int> SeedCentrosCostoBaseAsync(int empresaId);
}
