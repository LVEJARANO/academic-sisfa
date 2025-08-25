using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicoSFA.Repositories
{
    public class GrupoAcademicoRepository : IGrupoAcademicoRepository
    {
        private readonly SfaDbContext _context;

        public GrupoAcademicoRepository(SfaDbContext context)
        {
            _context = context;
        }

        public async Task<List<GrupoAcad>> ObtenerGruposAsync()
        {
            return await _context.GruposAcad
                .Include(g => g.Grado)
                .Include(g => g.Periodo)
                .OrderBy(g => g.Grado.NomGrado)
                .ThenBy(g => g.NomGrupo)
                .ToListAsync();
        }

        public async Task<List<GrupoAcad>> ObtenerGruposPorPeriodoAsync(int idPeriodo)
        {
            return await _context.GruposAcad
                .Include(g => g.Grado)
                .Include(g => g.Periodo)
                .Where(g => g.IdPeriodo == idPeriodo)
                .OrderBy(g => g.Grado.NomGrado)
                .ThenBy(g => g.NomGrupo)
                .ToListAsync();
        }

        public async Task<GrupoAcad> ObtenerGrupoPorIdAsync(int idGrupo)
        {
            return await _context.GruposAcad
                .Include(g => g.Grado)
                .Include(g => g.Periodo)
                .FirstOrDefaultAsync(g => g.Id == idGrupo);
        }

        public async Task<List<GrupoAcad>> ObtenerGruposPorGradoAsync(int idGrado)
        {
            return await _context.GruposAcad
                .Include(g => g.Grado)
                .Include(g => g.Periodo)
                .Where(g => g.IdGrado == idGrado)
                .OrderBy(g => g.NomGrupo)
                .ToListAsync();
        }

        public async Task<List<GrupoAcadMateria>> ObtenerMateriasDeGrupoAsync(int idGrupoAcad)
        {
            return await _context.GrupoAcadMateria
                .Where(gm => gm.IdGrupoAcad == idGrupoAcad)
                .Include(gm => gm.Materia)
                .ToListAsync();
        }
    }
}