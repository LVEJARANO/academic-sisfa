using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AcademicoSFA.Pages.Password
{
    [Authorize]
    public class CambiarContrasenaModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        private readonly IPasswordHasher<Domain.Entities.Participante> _passwordHasher;

        public CambiarContrasenaModel(
            SfaDbContext context,
            INotyfService servicioNotificacion,
            IPasswordHasher<Domain.Entities.Participante> passwordHasher)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        [Required(ErrorMessage = "La contrase�a actual es obligatoria")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La nueva contrase�a es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La confirmaci�n de contrase�a es obligatoria")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La nueva contrase�a y la confirmaci�n no coinciden.")]
        public string ConfirmPassword { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Solo aseg�rate de que el usuario est� autenticado
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/InicioSesion/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Obtener el ID del usuario autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _servicioNotificacion.Error("No se pudo identificar al usuario actual.");
                return RedirectToPage("/InicioSesion/Login");
            }

            // Buscar el participante en la base de datos
            var participante = await _context.Participantes
                .FirstOrDefaultAsync(p => p.Id == int.Parse(userId));

            if (participante == null)
            {
                _servicioNotificacion.Error("No se encontr� el usuario en el sistema.");
                return RedirectToPage("/InicioSesion/Login");
            }

            // Verificar la contrase�a actual
            var verificacionResult = _passwordHasher.VerifyHashedPassword(
                participante,
                participante.Clave,
                CurrentPassword);

            if (verificacionResult != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("CurrentPassword", "La contrase�a actual es incorrecta.");
                return Page();
            }

            // Hashear y guardar la nueva contrase�a
            participante.Clave = _passwordHasher.HashPassword(participante, NewPassword);
            _context.Participantes.Update(participante);
            await _context.SaveChangesAsync();

            _servicioNotificacion.Success("Tu contrase�a ha sido cambiada exitosamente.");
            return RedirectToPage("/Dashboard/Dashboard");
        }
    }
}