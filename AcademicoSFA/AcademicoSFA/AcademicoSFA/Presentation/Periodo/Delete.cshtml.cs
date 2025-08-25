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
using AcademicoSFA.Domain.Interfaces;

namespace AcademicoSFA.Pages.Periodo
{
    //[Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IPeriodoRepository _periodoRepository;
        private readonly INotyfService _servicioNotificacion;

        public DeleteModel(SfaDbContext context, IPeriodoRepository periodoRepository, INotyfService servicioNotificacion)
        {
            _context = context;
            _periodoRepository = periodoRepository;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.Periodo Periodo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Warning("El identificador del periodo no es válido.");
                return NotFound();
            }

            var periodo = await _context.Periodos.FirstOrDefaultAsync(m => m.Id == id);

            if (periodo == null)
            {
                _servicioNotificacion.Warning("El identificador del periodo no es válido.");
                return NotFound();
            }
            else
            {
                Periodo = periodo;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodo = await _context.Periodos.FindAsync(id);
            if (periodo != null)
            {
                try
                {
                    if (Periodo == null || Periodo.Id == 0)
                    {
                        _servicioNotificacion.Warning("El identificador del periodo no es válido.");
                        return RedirectToPage("./Index");
                    }
                    //
                    if (periodo.Activo == "SI")
                    {
                        await _periodoRepository.UpdateEstadoPeriodoAsync(periodo.Id);
                        _servicioNotificacion.Success("El periodo fue desactivado correctamente.");
                    }
                    else // Si el periodo está inactivo y se desea activar
                    {
                        // Validar si ya existe un periodo activo
                        var periodosActivos = await _periodoRepository.GetAllPeriodosActivosAsync();
                        if (periodosActivos.Count > 0)
                        {
                            _servicioNotificacion.Warning("No es posible activar este periodo porque ya existe un periodo activo. Por favor, desactive el periodo activo antes de proceder.");
                            return RedirectToPage("./Index");
                        }

                        // Activar el periodo
                        await _periodoRepository.UpdateEstadoPeriodoAsync(periodo.Id);
                        _servicioNotificacion.Success("El periodo fue activado correctamente.");
                    }
                }
                catch (Exception)
                {
                    _servicioNotificacion.Error("Ha ocurrido un error.");
                }
                return RedirectToPage("./Index");
            }

            return RedirectToPage("./Index");
        }
    }
}
