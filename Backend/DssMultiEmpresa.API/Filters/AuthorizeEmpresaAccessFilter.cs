using System.Security.Claims;
using DssMultiEmpresa.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Filters;

public class AuthorizeEmpresaAccessFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _db;

    public AuthorizeEmpresaAccessFilter(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values.TryGetValue("empresaId", out var empresaIdObj) &&
            int.TryParse(empresaIdObj?.ToString(), out int empresaId))
        {
            var user = context.HttpContext.User;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Usuario no autenticado." });
                return;
            }

            bool empresaPertenece = await _db.Empresas.AnyAsync(e => e.Id == empresaId && e.UsuarioId == usuarioId);
            if (!empresaPertenece)
            {
                context.Result = new ObjectResult(new { error = "Acceso denegado: No tiene autorización sobre la empresa especificada." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        await next();
    }
}

