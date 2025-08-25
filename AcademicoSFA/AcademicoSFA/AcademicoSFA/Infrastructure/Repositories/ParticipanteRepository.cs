using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MySqlConnector;
using System.Data;

namespace AcademicoSFA.Infrastructure.Repositories
{
    public class ParticipanteRepository: IParticipanteRepository
    {
        private readonly SfaDbContext _context;

        public ParticipanteRepository(SfaDbContext context)
        {
            _context = context;
        }
        public async Task<int> InsertParticipante(string nombres, string apellidos, string documento, string email, string rol)
        {
            try
            {
                // Ejecutar el procedimiento almacenado y obtener el resultado
                var result = await _context.Database.SqlQueryRaw<int>(
                    "CALL InsertParticipante(@p_nombres, @p_apellidos, @p_documento, @p_email, @p_rol)",
                    new MySqlParameter("@p_nombres", nombres),
                    new MySqlParameter("@p_apellidos", apellidos),
                    new MySqlParameter("@p_documento", documento),
                    new MySqlParameter("@p_email", email),
                    new MySqlParameter("@p_rol", rol)
                ).ToListAsync();

                // Verificar si se devolvió un resultado
                if (result.Any())
                {
                    return result.First(); // Retornar el ID del participante
                }

                return -1; // Si no se devolvió ningún resultado
            }
            catch (Exception ex)
            {
                // Manejar excepciones
                Console.WriteLine($"Error al insertar el participante: {ex.Message}");
                return -1;
            }
        }
        public async Task<List<Participante>> ObtenerParticipantePorDocumento(string documento)
        {

            // Ejecutar el procedimiento almacenado y mapear el resultado
            var participantes = await _context.Participantes
                .FromSqlRaw("CALL ObtenerParticipantePorDocumento(@p_documento)",
                    new MySqlParameter("@p_documento", documento))
                .ToListAsync();

            return participantes;
        }
        public async Task<List<Participante>> ObtenerParticipantePorId(int idPart)
        {

            // Ejecutar el procedimiento almacenado y mapear el resultado
            var participantes = await _context.Participantes
                .FromSqlRaw("CALL ObtenerParticipantePorID(@p_id_participante)",
                    new MySqlParameter("@p_id_participante", idPart))
                .ToListAsync();

            return participantes;
        }
        public async Task<int> UpdateParticipante(int idPart, string nombres, string apellidos, string documento, string email, string rol)
        {
            try
            {
                // Ejecutar el procedimiento almacenado y obtener el resultado
                var result = await _context.UpdateResults
                    .FromSqlRaw("CALL UpdateParticipante({0}, {1}, {2}, {3}, {4}, {5})",
                        idPart, nombres, apellidos, documento, email, rol)
                    .ToListAsync();

                // Leer el valor de rows_affected desde el resultado
                if (result.Any())
                {
                    return result.First().rows_affected;
                }

                return -1; // Si no se devolvió ningún resultado
            }
            catch (Exception ex)
            {
                // Manejar excepciones
                Console.WriteLine($"Error al actualizar el participante: {ex.Message}");
                return -1;
            }
        }

        public async Task UpdateDeleteParticipanteAsync(int idPart)
        {
            await _context.Database.ExecuteSqlRawAsync("CALL DeleteParticipante({0})", idPart);
        }

        // *** DOCENTES
        public async Task<List<Participante>> ObtenerDocentesAsync()
        {
            return await _context.Participantes
                .Where(a => a.Rol == "DOCENTE" && a.Eliminado == false)
                .ToListAsync();
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Participantes
                .Where(p => p.Email == email && (!p.Eliminado.HasValue || p.Eliminado == false))
                .AnyAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            return await _context.Alumnos
                .AnyAsync(a => a.Codigo == codigo);
        }
        //**** ADMIN
        public async Task<List<Participante>> ObtenerAdminAsync()
        {
            return await _context.Participantes
                .Where(a => a.Rol == "ADMIN" && a.Eliminado == false)
                .ToListAsync();
        }

        public async Task<List<Participante>> ObtenerParticipantesFiltradosAsync(string? terminoBusqueda, string rol)
        {
            var query = _context.Participantes
                .Where(p => p.Rol == rol && (p.Eliminado == false || !p.Eliminado.HasValue));

            if (!string.IsNullOrEmpty(terminoBusqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(terminoBusqueda) ||
                    p.Apellido.Contains(terminoBusqueda) ||
                    p.Documento.Contains(terminoBusqueda));
            }

            return await query.ToListAsync();
        }


    }
}

