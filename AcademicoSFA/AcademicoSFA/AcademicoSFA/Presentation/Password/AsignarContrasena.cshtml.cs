using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

// Usamos un alias para el modelo Participante para evitar conflictos
using ParticipanteModel = AcademicoSFA.Domain.Entities.Participante;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.Participante
{
    public class AsignarContrasenaModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        private readonly IPasswordHasher<ParticipanteModel> _passwordHasher;  // Ahora usamos el alias ParticipanteModel

        public AsignarContrasenaModel(SfaDbContext context, INotyfService servicioNotificacion, IPasswordHasher<ParticipanteModel> passwordHasher)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public int IdParticipante { get; set; }

        public List<SelectListItem> ParticipantesList { get; set; }  // Lista de participantes

        public async Task<IActionResult> OnGetAsync()
        {
            // Cargar la lista de participantes para el dropdown
            ParticipantesList = await _context.Participantes
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre + " " + p.Apellido
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validar si las contrase�as coinciden
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Las contrase�as no coinciden.");
                await OnGetAsync(); // Recargar la lista de participantes
                return Page();
            }

            // Buscar el participante seleccionado
            var participante = await _context.Participantes.FindAsync(IdParticipante);
            if (participante == null)
            {
                ModelState.AddModelError(string.Empty, "El participante no existe.");
                await OnGetAsync(); // Recargar la lista de participantes
                return Page();
            }

            // Hashear la contrase�a antes de guardarla
            participante.Clave = _passwordHasher.HashPassword(participante, Password);
            _context.Participantes.Update(participante);
            await _context.SaveChangesAsync();

            // Mostrar mensaje de �xito
            _servicioNotificacion.Success("La contrase�a se ha asignado correctamente.");
            return RedirectToPage("/Password/AsignarContrasena");
        }
    }
}
