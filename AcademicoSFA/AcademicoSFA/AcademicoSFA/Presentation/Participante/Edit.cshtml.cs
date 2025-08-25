using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Participante;

//[Authorize]
public class EditModel : PageModel
{
    private readonly SfaDbContext _context;
    private readonly ParticipanteRepository _repParticipante;
    private readonly INotyfService _servicioNotificacion;

    public EditModel(SfaDbContext context, ParticipanteRepository repParticipante, INotyfService servicioNotificacion)
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

        var participante =  await _context.Participantes.FirstOrDefaultAsync(m => m.Id == id);
        if (participante == null)
        {
            return NotFound();
        }
        Participante = participante;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (Participante != null)
            {
                int idPart = Participante.Id;
                string nombres = Participante.Nombre;
                string apellidos = Participante.Apellido;
                string documento = Participante.Documento;
                string email = Participante.Email;
                string rol = "DOCENTE";

                int insert=await _repParticipante.UpdateParticipante(idPart, nombres, apellidos, documento, email, rol);
                if (insert>0)
                {
                    _servicioNotificacion.Success("Docente actualizado exitosamente.");
                }
                else
                {
                    _servicioNotificacion.Error("No se pudo actualizar el docente.");
                }
            }
            else
            {
                _servicioNotificacion.Success("El docente no se encuentra registrado.");
            }
            return RedirectToPage("./Index");
        }
        catch (Exception)
        {
            _servicioNotificacion.Error("Ocurrió un error al intentar actualizar el docente.");
            return Page();

        }
    }
}
