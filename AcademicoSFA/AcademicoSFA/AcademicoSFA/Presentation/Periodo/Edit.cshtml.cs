using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AspNetCoreHero.ToastNotification.Abstractions;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Domain.Interfaces;

namespace AcademicoSFA.Pages.Periodo
{
    //[Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion; // Servicio de notificaciones
        private readonly IPeriodoRepository _periodoRepository;

        public EditModel(SfaDbContext context, INotyfService servicioNotificacion, IPeriodoRepository periodoRepository)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _periodoRepository = periodoRepository;
        }

        [BindProperty]
        public Domain.Entities.Periodo Periodo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodo =  await _context.Periodos.FirstOrDefaultAsync(m => m.Id == id);
            if (periodo == null)
            {
                return NotFound();
            }
            Periodo = periodo;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    _servicioNotificacion.Warning("Corrige los errores en el formulario antes de continuar.");
            //    return Page();
            //}
            try
            {

                await _periodoRepository.UpdateAnioPeriodoAsync(Periodo.Id, Periodo.AnioEscolar.ToString());
                _servicioNotificacion.Success("El año fue actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción de la base de datos
                _servicioNotificacion.Error($"Ocurrió un error al intentar actualizar la materia: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private bool PeriodoExists(int id)
        {
            return _context.Periodos.Any(e => e.Id == id);
        }
    }
}
