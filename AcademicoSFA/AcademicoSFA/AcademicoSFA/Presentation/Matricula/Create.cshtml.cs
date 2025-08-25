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
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly GradosGruposRepository _grupoService;
        private readonly INotyfService _servicioNotificacion;

        public CreateModel(SfaDbContext context, GradosGruposRepository grupoService, INotyfService servicioNotificacion)
        {
            _context = context;
            _grupoService = grupoService;
            _servicioNotificacion = servicioNotificacion;
        }
        [BindProperty]
        public int IdGrupoAcadSeleccionado { get; set; }
        public List<GradosGrupos> GradosGrupos { get; set; }
        public async Task OnGetAsync()
        {
            
            GradosGrupos = await _grupoService.ObtenerGradosGruposAsync();
        }

        [BindProperty]
        public MatriculaModels MatriculaModels { get; set; } = default!;

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
            // Verificar si el IdGrupoAcadSeleccionado existe en la tabla tbl_grupo_acad
            var grupoAcadExistente = await _context.GruposAcad.AnyAsync(g => g.Id == IdGrupoAcadSeleccionado);

            if (!grupoAcadExistente)
            {
                _servicioNotificacion.Error("El grupo académico seleccionado no es válido.");
                ModelState.AddModelError(string.Empty, "El grupo académico seleccionado no es válido.");
                return Page();
            }
            // Asignar el periodo activo y el estado "SI"
            MatriculaModels.IdPeriodo = periodoActivo.Id;
            MatriculaModels.Activo = "SI";
            MatriculaModels.IdGrupoAcad = IdGrupoAcadSeleccionado;
            // Agregar la matrícula
            _context.Matriculas.Add(MatriculaModels);
            await _context.SaveChangesAsync();
            _servicioNotificacion.Success("Matricula registrada exitosamente.");
            return RedirectToPage("./Index");
        }
    }
}
