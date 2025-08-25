using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.InicioSesion
{
    public class GoogleCallbackModel : PageModel
    {
        private readonly SfaDbContext _context;

        public GoogleCallbackModel(SfaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Esta es una mejor manera de verificar que Google haya autenticado al usuario
            var authenticateResult = await HttpContext.AuthenticateAsync("Google");
            if (!authenticateResult.Succeeded)
            {
                TempData["ErrorMessage"] = "La autenticación con Google falló. Por favor, intenta de nuevo.";
                return RedirectToPage("/InicioSesion/Login", new { errorMessage = "La autenticación con Google falló. Por favor, intenta de nuevo." });
            }
            try
            {
                // Obtenemos el email del usuario desde los claims de Google
                var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
                //var nombreCompleto = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario Google";

                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "No se pudo obtener el correo electrónico de tu cuenta de Google.";
                    return RedirectToPage("/InicioSesion/Login");
                }
                // Buscar por email (convertir ambos a minúsculas para comparación)
                var participante = await _context.Participantes
                    .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());

                if (participante == null)
                {
                    // Opción 1: Rechazar usuarios no registrados
                    TempData["ErrorMessage"] = "Este correo no está registrado en el sistema. Por favor, contacta al administrador.";

                    return RedirectToPage("/InicioSesion/Login", new { errorMessage = "Este correo no está registrado en el sistema. Por favor, contacta al administrador." });
                }
                // Crear los claims correctos incluyendo el NameIdentifier
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, participante.Nombre),
            new Claim(ClaimTypes.Email, participante.Email),
            new Claim(ClaimTypes.Role, participante.Rol),
            new Claim(ClaimTypes.NameIdentifier, participante.Id.ToString())

        };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Cerrar cualquier sesión previa
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                return RedirectToPage("/Dashboard/Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error en la autenticación: {ex.Message}";
                return RedirectToPage("/InicioSesion/Login", new { errorMessage = "Error en la autenticación: " + ex.Message });

            }

        }
    }
}
