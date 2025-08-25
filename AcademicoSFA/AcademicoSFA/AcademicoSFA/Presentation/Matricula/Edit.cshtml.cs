using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Matricula
{
    //[Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly GradosGruposRepository _grupoService;
        private readonly INotyfService _servicioNotificacion;

        public EditModel(SfaDbContext context, GradosGruposRepository grupoService, INotyfService servicioNotificacion)
        {
            _context = context;
            _grupoService = grupoService;
            _servicioNotificacion = servicioNotificacion;
        }
        [BindProperty]
        public int IdGrupoAcadSeleccionado { get; set; }
        public List<GradosGrupos> GradosGrupos { get; set; }

        [BindProperty]
        public MatriculaModels MatriculaModels { get; set; } = default!;
        public async Task OnGetAsync(int id)
        {
            GradosGrupos = await _grupoService.ObtenerGradosGruposAsync();
            MatriculaModels = await _context.Matriculas.FindAsync(id);
            if (MatriculaModels == null)
            {
                _servicioNotificacion.Error("no se encontro la matricula");
                RedirectToPage("./Index");
            }
            // Asignar el IdGrupoAcadSeleccionado si es necesario
            IdGrupoAcadSeleccionado = MatriculaModels.IdGrupoAcad;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            // Obtener el periodo activo de la base de datos
            var periodoActivo = await _context.Periodos.FirstOrDefaultAsync(p => p.Activo == "SI");

            if (periodoActivo == null)
            {
                _servicioNotificacion.Error("No hay un periodo activo actualmente.");
                ModelState.AddModelError(string.Empty, "No hay un periodo activo actualmente.");
                return Page();
            }
            // Asignar el periodo activo y el estado "SI"
            MatriculaModels.IdPeriodo = periodoActivo.Id;
            MatriculaModels.Activo = "SI";
            MatriculaModels.IdGrupoAcad = IdGrupoAcadSeleccionado;
            _context.Attach(MatriculaModels).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("La matrícula se ha actualizado correctamente.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatriculaModelsExists(MatriculaModels.IdMatricula))
                {
                    return NotFound();
                }
                else
                {
                    _servicioNotificacion.Error("Se ha producido un error al intentar guardar los cambios.");
                }
            }

            return RedirectToPage("./Index");
        }

        private bool MatriculaModelsExists(int id)
        {
            return _context.Matriculas.Any(e => e.IdMatricula == id);
        }
    }
}
