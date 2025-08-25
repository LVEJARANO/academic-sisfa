using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
namespace AcademicoSFA.Infrastructure.Repositories
{
    public class AlumnoRepository : IAlumnoRepository
    {
        private readonly SfaDbContext _context;
        public AlumnoRepository(SfaDbContext context)
        {
            _context = context;
        }
        public async Task<List<ActiveStudentDTO>> GetActiveStudents()
        {
            return await _context.Set<ActiveStudentDTO>()
                .FromSqlRaw("CALL GetStudents()")
                .ToListAsync();
        }
        public async Task<int> InsertAlumno(string codigo, int idParticipante, string estado)
        {
            try
            {
                // Ejecutar el procedimiento almacenado y obtener el resultado
                var result = await _context.Database.SqlQueryRaw<int>(
                    "CALL InsertAlumno(@p_codigo, @p_id_participante, @p_estado)",
                    new MySqlParameter("@p_codigo", codigo),
                    new MySqlParameter("@p_id_participante", idParticipante),
                    new MySqlParameter("@p_estado", estado)
                ).ToListAsync();
                if (result.Any())
                {
                    return result.First();
                }

                return 0; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar el alumno: {ex.Message}");
                return 0;
            }
        }
        public async Task UpdateEstadoAlumnoAsync(string codigo)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL UpdateStudentStatus({0})",codigo);
        }

        public async Task<List<ActiveStudentDTO>> ObtenerAlumnosFiltradosAsync(string? terminoBusqueda)
        {
            var alumnos = await _context.ActiveStudentDTO
                .FromSqlRaw("CALL GetStudents()")
                .ToListAsync();

            if (!string.IsNullOrEmpty(terminoBusqueda))
            {
                alumnos = alumnos
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.Nombre) && a.Nombre.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(a.Apellido) && a.Apellido.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(a.Codigo) && a.Codigo.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return alumnos;
        }
        public async Task<AlumnoModel?> ObtenerAlumnoConParticipantePorCodigoAsync(string codigo)
        {
            return await _context.Alumnos
                .Include(a => a.Participante)
                .FirstOrDefaultAsync(a => a.Codigo == codigo);
        }

        public async Task<bool> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
