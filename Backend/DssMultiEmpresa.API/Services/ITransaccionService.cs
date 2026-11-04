using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface ITransaccionService
{
    Task<IEnumerable<TransaccionResponseDto>> GetByEmpresaAsync(
        int empresaId,
        int? mes = null,
        int? anio = null,
        string? busqueda = null,
        int? cuentaContableId = null,
        string? codigoCuenta = null,
        int? terceroId = null,
        int? centroCostoId = null,
        string? tipo = null
    );

    Task<TransaccionResponseDto?> GetByIdAsync(int id, int empresaId);
    Task<TransaccionResponseDto> CreateAsync(int empresaId, TransaccionCreateDto dto);
    Task<bool> DeleteAsync(int id, int empresaId);
}
