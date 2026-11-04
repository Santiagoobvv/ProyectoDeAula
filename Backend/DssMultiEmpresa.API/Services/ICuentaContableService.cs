using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface ICuentaContableService
{
    Task<IEnumerable<CuentaContableResponseDto>> GetByEmpresaAsync(
        int empresaId,
        string? busqueda = null,
        int? clase = null,
        string? tipoCategoria = null,
        string? naturaleza = null,
        string? estado = null
    );

    Task<CuentaContableResponseDto?> GetByIdAsync(int id, int empresaId);
    Task<CuentaContableResponseDto?> GetByCodigoAsync(string codigo, int empresaId);
    Task<CuentaContableResponseDto> CreateAsync(int empresaId, CuentaContableCreateDto dto);
    Task<CuentaContableResponseDto?> UpdateAsync(int id, int empresaId, CuentaContableUpdateDto dto);
    Task<bool> ToggleEstadoAsync(int id, int empresaId);
    Task<bool> DeleteAsync(int id, int empresaId);
    Task<int> SeedPucBaseAsync(int empresaId);
}
