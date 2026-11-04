using System.ComponentModel.DataAnnotations;

namespace DssMultiEmpresa.API.DTOs;

// ── Auth ──
public record LoginRequest(
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password
);

public record RegisterRequest(
    [Required(ErrorMessage = "El nombre completo del contador es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre completo del contador solo debe contener letras y espacios.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    string Nombre,

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido.")]
    [StringLength(255, ErrorMessage = "El correo no puede superar 255 caracteres.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener un mínimo de 6 caracteres.")]
    string Password
);

public record AuthResponse(int UsuarioId, string Nombre, string Email, string Token);
