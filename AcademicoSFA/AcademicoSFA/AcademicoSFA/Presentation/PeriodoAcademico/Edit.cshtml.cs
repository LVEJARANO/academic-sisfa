using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace AcademicoSFA.Pages.PeriodoAcademico
{
    public class EditModel : PageModel
    {
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;
        private readonly IPeriodoRepository _periodoRepository;
        private readonly SfaDbContext _context;

        public EditModel(
            PeriodoAcademicoRepository periodoAcademicoRepository,
            IPeriodoRepository periodoRepository,
            SfaDbContext context)
        {
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _periodoRepository = periodoRepository;
            _context = context;
        }

        [BindProperty]
        public Domain.Entities.PeriodoAcademico PeriodoAcademico { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var periodoacademico = await _periodoAcademicoRepository.GetPeriodoAcademicoByIdAsync(id);
            if (periodoacademico == null)
            {
                return NotFound();
            }

            PeriodoAcademico = periodoacademico;

            // Cargar lista de periodos y preseleccionar el actual
            var periodos = await _periodoRepository.GetAllPeriodosAsync();
            ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar", PeriodoAcademico.IdPeriodo);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Verificar que la fecha fin sea mayor a la fecha inicio
                if (PeriodoAcademico.FechaFin <= PeriodoAcademico.FechaInicio)
                {
                    ModelState.AddModelError("PeriodoAcademico.FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
                    var periodos = await _periodoRepository.GetAllPeriodosAsync();
                    ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar", PeriodoAcademico.IdPeriodo);
                    return Page();
                }

                // MÉTODO DIRECTO: Usar SQL puro con el nombre correcto de la tabla y columnas
                string sql = @"
                    UPDATE tbl_periodo_academico 
                    SET id_periodo = @p1, 
                        numero_periodo = @p2, 
                        nombre = @p3, 
                        fecha_inicio = @p4, 
                        fecha_fin = @p5 
                    WHERE id_periodo_academico = @p0";

                // Ejecutar la consulta SQL con parámetros para evitar inyección SQL
                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    sql,
                    PeriodoAcademico.Id,
                    PeriodoAcademico.IdPeriodo,
                    PeriodoAcademico.NumeroPeriodo,
                    PeriodoAcademico.Nombre,
                    PeriodoAcademico.FechaInicio,
                    PeriodoAcademico.FechaFin);

                if (rowsAffected > 0)
                {
                    // Actualización exitosa
                    TempData["SuccessMessage"] = "Periodo académico actualizado correctamente.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    // No se actualizó ningún registro
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar el periodo académico. Verifique los datos.");
                    var periodos = await _periodoRepository.GetAllPeriodosAsync();
                    ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar", PeriodoAcademico.IdPeriodo);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Mostrar error al usuario
                ModelState.AddModelError(string.Empty, $"Error al actualizar: {ex.Message}");
                var periodos = await _periodoRepository.GetAllPeriodosAsync();
                ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar", PeriodoAcademico.IdPeriodo);
                return Page();
            }
        }
    }
}