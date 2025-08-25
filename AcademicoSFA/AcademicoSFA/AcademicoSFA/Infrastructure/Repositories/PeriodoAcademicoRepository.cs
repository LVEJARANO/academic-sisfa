using System.Diagnostics;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Infrastructure.Repositories
{
    public class PeriodoAcademicoRepository: IPeriodoRepository
    {
        private readonly SfaDbContext _context;

        public PeriodoAcademicoRepository(SfaDbContext context)
        {
            _context = context;
        }

        public async Task<List<PeriodoAcademico>> GetAllPeriodosAcademicosAsync()
        {
            return await _context.PeriodoAcademico
                .Include(p => p.Periodo)
                .OrderBy(p => p.IdPeriodo)
                .ThenBy(p => p.NumeroPeriodo)
                .ToListAsync();
        }

        public async Task<List<PeriodoAcademico>> GetPeriodosAcademicosByPeriodoIdAsync(int periodoId)
        {
            return await _context.PeriodoAcademico
                .Where(p => p.IdPeriodo == periodoId)
                .OrderBy(p => p.NumeroPeriodo)
                .ToListAsync();
        }

        public async Task<PeriodoAcademico> GetPeriodoAcademicoByIdAsync(int id)
        {
            return await _context.PeriodoAcademico
                .Include(p => p.Periodo)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreatePeriodoAcademicoAsync(PeriodoAcademico periodoAcademico)
        {
            _context.PeriodoAcademico.Add(periodoAcademico);
            await _context.SaveChangesAsync();
        }

        //public async Task UpdatePeriodoAcademicoAsync(PeriodoAcademico periodoAcademico)
        //{
        //    _context.Attach(periodoAcademico).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();
        //}
        // Método para actualizar un periodo académico existente
        public async Task<bool> UpdatePeriodoAcademicoAsync(PeriodoAcademico periodoAcademico)
        {
            try
            {
                // Opción 1: Utilizando Entity Framework directamente
                _context.Entry(periodoAcademico).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PeriodoAcademicoExistsAsync(periodoAcademico.Id))
                    {
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al actualizar periodo académico: {ex.Message}");
                return false;
            }
        }

        public async Task DeletePeriodoAcademicoAsync(int id)
        {
            var periodoAcademico = await _context.PeriodoAcademico.FindAsync(id);
            if (periodoAcademico != null)
            {
                _context.PeriodoAcademico.Remove(periodoAcademico);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PeriodoAcademicoExistsAsync(int id)
        {
            return await _context.PeriodoAcademico.AnyAsync(e => e.Id == id);
        }

        public async Task<List<Periodo>> GetActivePeriodosAsync()
        {
            return await _context.Periodos
                .Where(p => p.Activo == "SI")
                .OrderByDescending(p => p.AnioEscolar)
                .ToListAsync();
        }

        public async Task<int> ObtenerPeriodoAcademicoActualAsync()
        {
            // Primero obtenemos el periodo (año lectivo) activo
            var periodoActivo = await _context.Periodos
                .FirstOrDefaultAsync(p => p.Activo == "SI");

            if (periodoActivo == null)
            {
                // Si no hay periodo activo, retornamos 0 o lanzamos una excepción
                return 0;
            }

            // Obtenemos la fecha actual
            var fechaActual = DateTime.Now.Date;

            // Buscamos el periodo académico que corresponde a la fecha actual
            var periodoAcademico = await _context.PeriodoAcademico
                .Where(pa => pa.IdPeriodo == periodoActivo.Id)
                .Where(pa => pa.FechaInicio <= fechaActual && pa.FechaFin >= fechaActual)
                .FirstOrDefaultAsync();

            if (periodoAcademico != null)
            {
                return periodoAcademico.Id;
            }

            // Si no hay un periodo académico para la fecha actual,
            // retornamos el último periodo académico del periodo activo
            var ultimoPeriodoAcademico = await _context.PeriodoAcademico
                .Where(pa => pa.IdPeriodo == periodoActivo.Id)
                .OrderByDescending(pa => pa.FechaFin)
                .FirstOrDefaultAsync();

            return ultimoPeriodoAcademico?.Id ?? 0;
        }

        // Versión sincrónica del método para casos donde no se pueda usar async/await
        public int ObtenerPeriodoAcademicoActual()
        {
            // Primero obtenemos el periodo (año lectivo) activo
            var periodoActivo = _context.Periodos
                .FirstOrDefault(p => p.Activo == "SI");

            if (periodoActivo == null)
            {
                // Si no hay periodo activo, retornamos 0 o lanzamos una excepción
                return 0;
            }

            // Obtenemos la fecha actual
            var fechaActual = DateTime.Now.Date;

            // Buscamos el periodo académico que corresponde a la fecha actual
            var periodoAcademico = _context.PeriodoAcademico
                .Where(pa => pa.IdPeriodo == periodoActivo.Id)
                .Where(pa => pa.FechaInicio <= fechaActual && pa.FechaFin >= fechaActual)
                .FirstOrDefault();

            if (periodoAcademico != null)
            {
                return periodoAcademico.Id;
            }

            // Si no hay un periodo académico para la fecha actual,
            // retornamos el último periodo académico del periodo activo
            var ultimoPeriodoAcademico = _context.PeriodoAcademico
                .Where(pa => pa.IdPeriodo == periodoActivo.Id)
                .OrderByDescending(pa => pa.FechaFin)
                .FirstOrDefault();

            return ultimoPeriodoAcademico?.Id ?? 0;
        }


        //*****************************
        public async Task<List<Periodo>> GetAllPeriodosAsync()
        {
            return await _context.Periodos
                .FromSqlRaw("CALL GetAllPeriodos()")
                .ToListAsync();
        }
        public async Task<List<Periodo>> GetAllPeriodosActivosAsync()
        {
            return await _context.Periodos
                .FromSqlRaw("CALL GetAllPeriodosActivos()")
                .ToListAsync();
        }
        public async Task UpdateEstadoPeriodoAsync(int id_periodo)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL UpdateEstadoPeriodo({0})", id_periodo);
        }
        public async Task UpdateAnioPeriodoAsync(int idPeriodo, string anio)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL UpdateAnioPeriodo({0}, {1})", idPeriodo, anio);
        }


    }
}
