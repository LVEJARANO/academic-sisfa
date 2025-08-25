using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.DocenteMateriaGrupoAcad
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        private readonly ParticipanteRepository _participanteRepository;
        private readonly MateriaRepository _materiaRepository;
        private readonly GradosGruposRepository _gradosGruposRepository;
        public CreateModel(SfaDbContext context, INotyfService servicioNotificacion, ParticipanteRepository participanteRepository, MateriaRepository materiaRepository, GradosGruposRepository gradosGruposRepository)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _participanteRepository = participanteRepository;
            _materiaRepository = materiaRepository;
            _gradosGruposRepository = gradosGruposRepository;
        }
        [BindProperty]
        public List<MateriasADocentesModels> MateriasADocentes { get; set; }
        [BindProperty]
        public int IdpartDoc { get; set; }
        public List<Domain.Entities.Participante> Docentes { get; set; }
        public List<Domain.Entities.Materia> Materias { get; set; }
        public List<GradosGrupos> GradosGrupos { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Docentes = await _participanteRepository.ObtenerDocentesAsync();
                Materias = await _materiaRepository.GetAllMateriasAsync();
                GradosGrupos = await _gradosGruposRepository.ObtenerGradosGruposAsync();
            }
            catch (Exception ex)
            {
                _servicioNotificacion.Error("Ocurrió un error al cargar los datos iniciales. Por favor, inténtelo de nuevo.");
                return RedirectToPage("./Index");
            }

            return Page();
        }

        [BindProperty]
        public Domain.Entities.DocenteMateriaGrupoAcad DocenteMateriaGrupoAcad { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Error("Por favor, corrija los errores en el formulario antes de continuar.");
                await OnGetAsync();
                return Page();
            }
            DocenteMateriaGrupoAcad.IdParticipante = IdpartDoc;
            try
            {
                var existeAsignacion = await _context.DocenteMateriasGrupoAcad
                    .AnyAsync(d => d.IdParticipante == DocenteMateriaGrupoAcad.IdParticipante
                                   && d.IdMateria == DocenteMateriaGrupoAcad.IdMateria
                                   && d.IdGrupoAcad == DocenteMateriaGrupoAcad.IdGrupoAcad);

                if (existeAsignacion)
                {
                    _servicioNotificacion.Warning("Esta asignación ya existe. El docente ya está asignado a esta materia y grupo.");
                    await OnGetAsync();
                    return Page();
                }

                _context.DocenteMateriasGrupoAcad.Add(DocenteMateriaGrupoAcad);
                await _context.SaveChangesAsync();

                _servicioNotificacion.Success("La asignación del docente a la materia y grupo se ha registrado correctamente.");
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _servicioNotificacion.Error("Ocurrió un error al guardar la asignación. Por favor, inténtelo de nuevo.");
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _servicioNotificacion.Error("Ocurrió un error inesperado. Por favor, contacte al administrador.");
                await OnGetAsync();
                return Page();
            }

        }
    }

}
