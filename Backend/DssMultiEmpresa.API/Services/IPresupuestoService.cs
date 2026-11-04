using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface IPresupuestoService
{
    Task<IEnumerable<PresupuestoResponseDto>> GetByEmpresaAsync(
        int empresaId,
        int? mes = null,
        int? anio = null,
        string? busqueda = null,
        string? categoria = null,
        string? tipoPresupuesto = null,
        string? estado = null
    );

    Task<PresupuestoResponseDto?> GetByIdAsync(int id, int empresaId);
    Task<PresupuestoResponseDto> CreateAsync(int empresaId, PresupuestoCreateDto dto);
    Task<PresupuestoResponseDto?> UpdateAsync(int id, int empresaId, PresupuestoUpdateDto dto);
    Task<bool> DeleteAsync(int id, int empresaId);
}
