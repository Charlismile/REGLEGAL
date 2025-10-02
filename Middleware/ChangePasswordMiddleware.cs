using Microsoft.AspNetCore.Identity;
using REGISTROLEGAL.Data;

namespace REGISTROLEGAL.Middleware;

public class ChangePasswordMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly PathString ChangePasswordPath = new("/Account/Manage/Change-password");

    private static readonly string[] AllowedPrefixes =
    {
        "/_blazor",
        "/_framework",
        "/_content",
        "/_nuxt",
        "/css",
        "/js",
        "/images",
        "/favicon.ico",
        "/Account/Login",
        "/Account/Logout",
        "/Account/Manage/Change-password",
        "/api"
    };

    public ChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private static bool IsAllowedPath(PathString path)
    {
        var p = path.Value ?? string.Empty;
        return AllowedPrefixes.Any(pref => p.StartsWith(pref, StringComparison.OrdinalIgnoreCase));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Solo aplica a usuarios autenticados
        if (!(context.User?.Identity?.IsAuthenticated ?? false))
        {
            await _next(context);
            return;
        }

        // No interferir con recursos estáticos, Blazor hub, endpoints permitidos, etc.
        if (IsAllowedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Solo redirigir en navegaciones GET (evita POST, SignalR, etc.)
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Obtener UserManager desde el contenedor de servicios
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        // Validar si el usuario debe cambiar la contraseña
        if (user != null && user.MustChangePassword)
        {
            context.Response.Redirect(ChangePasswordPath);
            return;
        }

        // Continuar con la siguiente etapa del pipeline
        await _next(context);
    }
}
