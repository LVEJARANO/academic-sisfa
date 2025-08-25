using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicoSFA.Pages.Administrativo
{
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IParticipanteRepository _repParticipante;
        private readonly INotyfService _servicioNotificacion;
        public CreateModel(SfaDbContext context, IParticipanteRepository repParticipante, INotyfService servicioNotificacion)
        {
            _context = context;
            _repParticipante = repParticipante;
            _servicioNotificacion = servicioNotificacion;
        }
        [BindProperty]
        public Domain.Entities.Participante Participante { get; set; } = default!;
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var part = await _repParticipante.ObtenerParticipantePorDocumento(Participante.Documento);
                int idPart;
                if (part.Count > 0)
                {
                    idPart = part[0].Id;
                    _servicioNotificacion.Success("El administrativo ya se encuentra registrado.");
                }
                else
                {
                    string nombres = Participante.Nombre;
                    string apellidos = Participante.Apellido;
                    string documento = Participante.Documento;
                    string email = Participante.Email;
                    string rol = "ADMIN";

                    idPart = await _repParticipante.InsertParticipante(nombres, apellidos, documento, email, rol);
                    _servicioNotificacion.Success("Administrativo creado exitosamente.");
                }
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _servicioNotificacion.Error("Ocurrió un error al intentar crear el administrativo.");
                return Page();
            }
        }
    }
}
