using DssMultiEmpresa.API.DTOs;

namespace DssMultiEmpresa.API.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}
