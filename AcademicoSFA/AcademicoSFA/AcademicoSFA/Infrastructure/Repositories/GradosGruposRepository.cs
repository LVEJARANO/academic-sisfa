using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Infrastructure.Repositories
{
    public class GradosGruposRepository: IGradosGruposRepository
    {
        private readonly SfaDbContext _context;
        public GradosGruposRepository(SfaDbContext context)
        {
            _context = context;
        }
        //Metodo que obtiene los grados concatenados con los grupos ejemplo: 1-A,2-A
        public async Task<List<GradosGrupos>> ObtenerGradosGruposAsync()
        {
            return await _context.GradosGruposInfo
                .FromSqlRaw("CALL ObtenerGradosGrupos()")
                .ToListAsync();
        }
    }
}
