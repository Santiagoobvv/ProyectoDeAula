using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface ITerceroService
{
    Task<IEnumerable<TerceroResponseDto>> GetByEmpresaAsync(int empresaId, string? busqueda = null, string? tipoTercero = null);
    Task<TerceroResponseDto?> GetByIdAsync(int id, int empresaId);
    Task<TerceroResponseDto?> GetByDocumentoAsync(string numeroDocumento, int empresaId);
    Task<TerceroResponseDto> CreateAsync(int empresaId, TerceroCreateDto dto);
    Task<TerceroResponseDto?> UpdateAsync(int id, int empresaId, TerceroUpdateDto dto);
    Task<bool> DeleteAsync(int id, int empresaId);
}
