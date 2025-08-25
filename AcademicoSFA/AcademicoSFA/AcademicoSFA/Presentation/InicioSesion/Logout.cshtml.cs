using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AcademicoSFA.Pages.InicioSesion
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
            // Mostramos la página de confirmación sin realizar acciones aún
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Cerrar sesión del esquema de cookies (que es el único que soporta SignOutAsync)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Limpiar cookies relacionadas con la sesión
            Response.Cookies.Delete(".AspNetCore.Cookies");
            Response.Cookies.Delete(".AspNetCore.Session");

            // Opcional: Redirigir a la página de desconexión de Google después de cerrar sesión local
            return Redirect("https://www.google.com/accounts/Logout?continue=https://appengine.google.com/_ah/logout");

            // Alternativamente, simplemente volver a la página de inicio de sesión:
            // return RedirectToPage("/InicioSesion/Login");
        }
    }
}