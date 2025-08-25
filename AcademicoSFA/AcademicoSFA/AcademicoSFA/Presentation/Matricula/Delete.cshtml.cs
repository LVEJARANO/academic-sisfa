using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AspNetCoreHero.ToastNotification.Abstractions;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Pages.Matricula
{
 //   [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly GradosGruposRepository _grupoService;
        private readonly INotyfService _servicioNotificacion;
        private readonly MatriculaRepository _matriculaService;

        public DeleteModel(SfaDbContext context, GradosGruposRepository grupoService, INotyfService servicioNotificacion, MatriculaRepository matriculaService)
        {
            _context = context;
            _grupoService = grupoService;
            _servicioNotificacion = servicioNotificacion;
            _matriculaService = matriculaService;
        }
        [BindProperty]
        public int IdGrupoAcadSeleccionado { get; set; }
        public List<GradosGrupos> GradosGrupos { get; set; }
        [BindProperty]
        public MatriculaModels MatriculaModels { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("El ID de matrícula no ha sido proporcionado.");
                return NotFound();
            }

            var matriculamodels = await _context.Matriculas.FirstOrDefaultAsync(m => m.IdMatricula == id);

            if (matriculamodels == null)
            {
                _servicioNotificacion.Error("No se ha encontrado ninguna matrícula con el ID proporcionado.");
                return NotFound();
            }
            else
            {
                MatriculaModels = matriculamodels;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("El ID de matrícula no ha sido proporcionado.");
                return NotFound();
            }

            var matriculamodels = await _context.Matriculas.FindAsync(id);
            if (matriculamodels != null)
            {
                MatriculaModels = matriculamodels;
                await _matriculaService.UpdateEstadoMatriculaAsync(MatriculaModels.Codigo);
                _servicioNotificacion.Success("La matrícula se ha eliminado exitosamente.");
            }
            else
            {
                _servicioNotificacion.Error("No se encontró una matrícula con el ID proporcionado.");
            }

            return RedirectToPage("./Index");
        }
    }
}
