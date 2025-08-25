using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Alumno;
//[Authorize]
public class DeleteModel : PageModel
{
    private readonly IAlumnoRepository _alumno;
    private readonly INotyfService _servicioNotificacion;
    public DeleteModel(IAlumnoRepository alumno, INotyfService servicioNotificacion)
    {
        _servicioNotificacion = servicioNotificacion;
        _alumno = alumno;
    }

    [BindProperty]
    public AlumnoModel Alumno { get; set; } = default!;
    [BindProperty]
    public Domain.Entities.Participante Participantes { get; set; } = default!;
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        try
        {
            if (id == null)
            {
                _servicioNotificacion.Error("No se ha encontrado el alumno");
                return NotFound();
            }
            var alumnos = await _alumno.ObtenerAlumnoConParticipantePorCodigoAsync(id.ToString());
            if (alumnos == null)
            {
                _servicioNotificacion.Error("No se econtró la información del alumno.");
                return NotFound();
            }
            else
            {
                Alumno = alumnos;
                Participantes = Alumno.Participante; // Almacena el participante encontrado

            }
        }
        catch (Exception ex)
        {
            _servicioNotificacion.Error("Ha ocurrido un error.");
            Console.WriteLine($"[ERROR] Eliminando alumno: {ex.Message}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {

        try
        {
            var alumno = await _alumno.ObtenerAlumnoConParticipantePorCodigoAsync(Alumno.Codigo);
            if (alumno == null)
            {
                _servicioNotificacion.Error("El alumno ya no existe.");
                return RedirectToPage("./Index");
            }

            await _alumno.UpdateEstadoAlumnoAsync(Alumno.Codigo);
            _servicioNotificacion.Success("Se eliminó el alumno correctamente.");
        }
        catch (Exception ex)
        {
            _servicioNotificacion.Error("Ha ocurrido un error.");
            Console.WriteLine($"[ERROR] Eliminando alumno: {ex.Message}");
        }
        return RedirectToPage("./Index");
    }
}
