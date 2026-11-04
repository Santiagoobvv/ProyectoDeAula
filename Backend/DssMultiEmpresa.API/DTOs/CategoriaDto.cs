namespace DssMultiEmpresa.API.DTOs;

// ── Categoría Contable ──
public record CategoriaCreateDto(string Nombre, string Tipo);
public record CategoriaResponseDto(int Id, int EmpresaId, string Nombre, string Tipo);
