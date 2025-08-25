using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Participante;
//[Authorize]
public class DeleteModel : PageModel
{
    private readonly SfaDbContext _context;
    private readonly ParticipanteRepository _repParticipante;
    private readonly INotyfService _servicioNotificacion;

    public DeleteModel(SfaDbContext context, ParticipanteRepository repParticipante, INotyfService servicioNotificacion)
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
            return NotFound();
        }

        var participante = await _context.Participantes.FirstOrDefaultAsync(m => m.Id == id);

        if (participante == null)
        {
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
        if (id == null)
        {
            _servicioNotificacion.Error("Ocurrió un error al intentar eliminar el docente.");
            return NotFound();
        }
        await _repParticipante.UpdateDeleteParticipanteAsync(Participante.Id);
        _servicioNotificacion.Success("Docente eliminado exitosamente.");
        return RedirectToPage("./Index");
    }
}
