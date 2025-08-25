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
            // Mostramos la p�gina de confirmaci�n sin realizar acciones a�n
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Cerrar sesi�n del esquema de cookies (que es el �nico que soporta SignOutAsync)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Limpiar cookies relacionadas con la sesi�n
            Response.Cookies.Delete(".AspNetCore.Cookies");
            Response.Cookies.Delete(".AspNetCore.Session");

            // Opcional: Redirigir a la p�gina de desconexi�n de Google despu�s de cerrar sesi�n local
            return Redirect("https://www.google.com/accounts/Logout?continue=https://appengine.google.com/_ah/logout");

            // Alternativamente, simplemente volver a la p�gina de inicio de sesi�n:
            // return RedirectToPage("/InicioSesion/Login");
        }
    }
}