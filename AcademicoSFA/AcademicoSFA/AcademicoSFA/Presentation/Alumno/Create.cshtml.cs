using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicoSFA.Pages.Alumno;
//[Authorize]
public class CreateModel : PageModel
{
    private readonly SfaDbContext _context;
    private readonly IParticipanteRepository _repParticipante;
    private readonly IAlumnoRepository _repAlumno;
    private readonly INotyfService _servicioNotificacion;

    public CreateModel(SfaDbContext context, IParticipanteRepository repParticipante, IAlumnoRepository repAlumno, 
                      INotyfService servicioNotificacion)
    {
        _context = context;
        _servicioNotificacion = servicioNotificacion;
        _repParticipante = repParticipante;
        _repAlumno = repAlumno;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    [BindProperty]
    public AlumnoModel Alumno { get; set; } = default!;

    [BindProperty]
    public Domain.Entities.Participante Participante { get; set; } = default!;
    public async Task<IActionResult> OnPostAsync()
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {

                var idParticipante = await _repParticipante.ObtenerParticipantePorDocumento(Participante.Documento);
                int idPart;
                if (idParticipante.Count > 0)
                {
                    idPart = idParticipante[0].Id;
                }
                else
                {

                    string nombres = Participante.Nombre;
                    string apellidos = Participante.Apellido;
                    string documento = Participante.Documento;
                    string email = Participante.Email;
                    string rol = "ESTUDIANTE";

                    idPart = await _repParticipante.InsertParticipante(nombres, apellidos, documento, email, rol);


                    if (idPart <= 0)
                    {
                        _servicioNotificacion.Error("Es necesario corregir los problemas en el formulario del participante.");
                        return Page();
                    }
                }
                string codigo = Alumno.Codigo;
                string estado = "Activo";

                int resultado = await _repAlumno.InsertAlumno(codigo, idPart, estado);

                if (resultado == 1)
                {
                    await transaction.CommitAsync();
                    _servicioNotificacion.Success("Alumno creado exitosamente.");
                    return RedirectToPage("./Index");
                }
                else
                {
                    await transaction.RollbackAsync();
                    _servicioNotificacion.Error("Es necesario corregir los problemas en el formulario del alumno.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error, hacer rollback de la transacción
                await transaction.RollbackAsync();
                _servicioNotificacion.Error("Ocurrió un error al intentar crear el alumno. El proceso ha sido revertido.");
                return Page();
            }
        }
    }
}
