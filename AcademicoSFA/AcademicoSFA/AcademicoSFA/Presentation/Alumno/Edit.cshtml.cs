using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Alumno;
//[Authorize]
public class EditModel : PageModel
{
    private readonly INotyfService _servicioNotificacion;
    private readonly IAlumnoRepository _repAlumno;

    public EditModel(INotyfService servicioNotificacion, IAlumnoRepository repAlumno)
    {
        _servicioNotificacion = servicioNotificacion;
        _repAlumno = repAlumno;
    }

    [BindProperty]
    public AlumnoModel Alumno { get; set; } = default!;

    [BindProperty]
    public Domain.Entities.Participante Participantes { get; set; } = default!;
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var alumnos = await _repAlumno.ObtenerAlumnoConParticipantePorCodigoAsync(id.ToString());
        if (alumnos == null)
        {
            return NotFound();
        }
        Alumno = alumnos;
        Participantes = Alumno.Participante;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var alumno = await _repAlumno.ObtenerAlumnoConParticipantePorCodigoAsync(id.ToString());

            if (alumno == null || alumno.Participante == null)
            {
                _servicioNotificacion.Error("No se ha encontrado el alumno");
                return NotFound();
            }

            // Actualiza las propiedades del participante existente
            var participante = alumno.Participante;
            participante.Nombre = Participantes.Nombre;
            participante.Apellido = Participantes.Apellido;
            participante.Documento = Participantes.Documento;

            // Actualiza las propiedades del alumno
            alumno.Codigo = Alumno.Codigo;
            alumno.Estado = alumno.Estado;
            // Guarda todos los cambios en la base de datos
            await _repAlumno.GuardarCambiosAsync();
            _servicioNotificacion.Success("El alumno se ha modificado exitosamente.");
        }
        catch (DbUpdateConcurrencyException)
        {
            _servicioNotificacion.Error("Ha ocurrido un error.");
        }
        return RedirectToPage("./Index");
    }
}
