using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Administrativo
{
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IParticipanteRepository _repParticipante;
        private readonly INotyfService _servicioNotificacion;

        public DeleteModel(SfaDbContext context, IParticipanteRepository repParticipante, INotyfService servicioNotificacion)
        {
            _context = context;
            _repParticipante = repParticipante;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.Participante Participante { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("No se encontró el administrativo.");
                return NotFound();
            }

            var participante = await _context.Participantes.FirstOrDefaultAsync(m => m.Id == id);

            if (participante == null)
            {
                _servicioNotificacion.Error("No se encontró el administrativo.");
                return NotFound();
            }
            else
            {
                Participante = participante;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    _servicioNotificacion.Error("Ocurrió un error al intentar eliminar el docente.");
                    return NotFound();
                }
                await _repParticipante.UpdateDeleteParticipanteAsync(Participante.Id);
                _servicioNotificacion.Success("Docente eliminado exitosamente.");
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                _servicioNotificacion.Error("Ocurrió un error al intentar eliminar el administrativo.");
                return Page();
            }
        }
    }
}
