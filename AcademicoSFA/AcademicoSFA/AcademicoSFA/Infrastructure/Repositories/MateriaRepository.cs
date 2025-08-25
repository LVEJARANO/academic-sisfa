using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Infrastructure.Repositories
{
    public class MateriaRepository : IMateriaRepository
    {
        private readonly SfaDbContext _context;
        public MateriaRepository(SfaDbContext context)
        {
            _context = context;
        }
        // Método para obtener todas las materias
        public async Task<List<Materia>> GetAllMateriasAsync()
        {
            return await _context.Materias
                .FromSqlRaw("CALL GetAllMaterias()")
                .ToListAsync();
        }

        // Método para obtener materias por nombre
        public async Task<List<Materia>> GetMateriasByNombreAsync(string nombreMateria)
        {
            return await _context.Materias
                .FromSqlRaw("CALL GetMateriasByNombre({0})", nombreMateria)
                .ToListAsync();
        }
        // Método para obtener materias por id

        public async Task<Materia?> GetMateriaByIdAsync(int idMateria)
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "CALL GetMateriaById(@id)";
            command.CommandType = System.Data.CommandType.Text;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = idMateria;
            command.Parameters.Add(parameter);

            await _context.Database.OpenConnectionAsync();

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Materia
                {
                    Id = reader.GetInt32(0), // Ajustar el índice según las columnas
                    NomMateria = reader.GetString(1)
                };
            }

            return null;
        }
        // Método para insertar una materia usando el procedimiento almacenado
        public async Task InsertMateriaAsync(string nombreMateria)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL InsertMateria({0})", nombreMateria);
        }

        public async Task UpdateMateriaAsync(int id, string nombreMateria)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL UpdateMateria({0}, {1})", id, nombreMateria);
        }

        public async Task<int> ObtenerIdMateria(string nombreMateria)
        {
            var materia = await _context.Materias
                .Where(m => m.NomMateria.Equals(nombreMateria))
                .FirstOrDefaultAsync();

            if (materia != null)
            {
                return materia.Id;
            }

            throw new Exception($"No se encontró ninguna materia con el nombre '{nombreMateria}'");
        }
        // Métodos adicionales de materias asignadas a docentes
        public async Task<List<MateriasADocentesModels>> GetMateriasAsignadasADocentes()
        {
            return await _context.Set<MateriasADocentesModels>()
                .FromSqlRaw("CALL AsignarMateriasADocentes()")
                .ToListAsync();
        }

        public async Task<DocenteMateriaGrupoAcad> GetDocenteMateriaGrupoAcadDetalladoAsync(int id)
        {
            return await _context.DocenteMateriasGrupoAcad
                .Include(dmg => dmg.Participante)
                .Include(dmg => dmg.Materia)
                .Include(dmg => dmg.GrupoAcad)
                .ThenInclude(g => g.Grado)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
