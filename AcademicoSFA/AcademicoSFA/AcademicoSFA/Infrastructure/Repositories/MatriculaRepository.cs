using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Infrastructure.Repositories
{
    public class MatriculaRepository : IMatriculaRepository
    {
        private readonly SfaDbContext _context;

        public MatriculaRepository(SfaDbContext context)
        {
            _context = context;
        }

        public async Task<List<MatriculaInfo>> ObtenerMatriculasActivasAsync()
        {
            return await _context.MatriculasInfo
                .FromSqlRaw("CALL ObtenerMatriculasActivas()")
                .ToListAsync();
        }
        public async Task<List<MatriculaPagosModels>> ObtenerMatriculasConPagosAsync()
        {
            return await _context.MatriculaPagosModels
                .FromSqlRaw("CALL ObtenerMatriculasConPagos()")
                .ToListAsync();
        }
        public async Task UpdateEstadoMatriculaAsync(string codigo)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL UpdateMatriculaStatus({0})", codigo);
        }
        public async Task<int> ObtenerIdMatricula(string codigo)
        {
            var idMatricula = await _context.Matriculas
                .Where(m => m.Codigo == codigo)
                .Select(m => m.IdMatricula)
                .FirstOrDefaultAsync();

            if (idMatricula == 0) // Si no se encontró ninguna matrícula con ese código
            {
                throw new KeyNotFoundException($"No se encontró matrícula con el código {codigo}");
            }

            return idMatricula;
        }
        public async Task<string> ObtenerNumeroMatriculaParticipante(int idParticipante)
        {
            var matricula = await _context.Participantes
                .Where(part => part.Id == idParticipante)
                .Join(
                    _context.Alumnos,
                    part => part.Id,
                    al => al.IdParticipante,
                    (part, al) => al.Codigo
                )
                .Join(
                    _context.Matriculas,
                    codigo => codigo,
                    mat => mat.Codigo,
                    (codigo, mat) => mat
                )
                .Where(mat => mat.Activo.ToLower() == "si")
                .FirstOrDefaultAsync();

            return matricula?.IdMatricula.ToString(); // Devuelve el código de matrícula o null si no existe
        }
        public async Task<MatriculaModels> ObtenerMatriculaConGradoPorCodigoEstudiante(string codigoEstudiante)
        {
            return await _context.Matriculas
                .Include(m => m.GruposAcad)
                    .ThenInclude(g => g.Grado)
                .FirstOrDefaultAsync(m => m.Codigo == codigoEstudiante);
        }

    }
}
