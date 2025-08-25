// Agrega estas líneas al inicio del archivo
using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

// Modifica la clase NotasRepository
public class NotasRepository : INotasRepository
{
    private readonly SfaDbContext _context;
    private readonly string _connectionString;
    private readonly ILogger<NotasRepository> _logger; // ← NUEVA LÍNEA

    public NotasRepository(SfaDbContext context, ILogger<NotasRepository> logger) // ← MODIFICAR CONSTRUCTOR
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString();
        _logger = logger; // ← NUEVA LÍNEA
    }

    public async Task<int> GuardarNotaAsync(
        int idMatricula,
        int idMateria,
        int idPeriodoAcademico,
        decimal? notaSaber,
        decimal? notaHacer,
        decimal? notaSer,
        string observacion)
    {
        var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

        try
        {
            _logger.LogInformation("Guardando nota para matrícula {IdMatricula}, materia {IdMateria}",
                idMatricula, idMateria); // ← NUEVA LÍNEA

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("sp_InsertarNota", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("p_id_matricula", idMatricula);
                    command.Parameters.AddWithValue("p_id_materia", idMateria);
                    command.Parameters.AddWithValue("p_id_periodo_academico", idPeriodoAcademico);
                    command.Parameters.AddWithValue("p_nota_saber", notaSaber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("p_nota_hacer", notaHacer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("p_nota_ser", notaSer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("p_observacion", string.IsNullOrEmpty(observacion) ? DBNull.Value : observacion);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var idNota = reader.GetInt32("id_nota");

                            _logger.LogInformation("Nota guardada exitosamente con ID {IdNota} en {Duration}ms",
                                idNota, stopwatch.ElapsedMilliseconds); // ← NUEVA LÍNEA

                            return idNota;
                        }
                        return 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando nota para matrícula {IdMatricula}, materia {IdMateria}",
                idMatricula, idMateria); // ← NUEVA LÍNEA
            throw;
        }
    }

    public async Task<List<NotaModels>> ObtenerNotasPorMatricula(int idMatricula)
    {
        var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

        try
        {
            _logger.LogInformation("Obteniendo notas para matrícula {IdMatricula}", idMatricula); // ← NUEVA LÍNEA

            var result = await _context.NotaModels
                .Where(n => n.IdMatricula == idMatricula)
                .Include(n => n.Materia)
                .Include(n => n.PeriodoAcademico)
                .Include(n => n.Matricula)
                .ToListAsync();

            _logger.LogInformation("Obtenidas {Count} notas para matrícula {IdMatricula} en {Duration}ms",
                result.Count, idMatricula, stopwatch.ElapsedMilliseconds); // ← NUEVA LÍNEA

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo notas para matrícula {IdMatricula}", idMatricula); // ← NUEVA LÍNEA
            throw;
        }
    }

    // Continuar con los demás métodos igual...
    public async Task GuardarObservacion(
        int idMatricula,
        int idMateria,
        int idPeriodoAcademico,
        string observacion)
    {
        var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

        try
        {
            _logger.LogInformation("Guardando observación para matrícula {IdMatricula}, materia {IdMateria}",
                idMatricula, idMateria); // ← NUEVA LÍNEA

            var notaExistente = await _context.NotaModels
                .FirstOrDefaultAsync(n => n.IdMatricula == idMatricula &&
                                     n.IdMateria == idMateria &&
                                     n.IdPeriodoAcademico == idPeriodoAcademico &&
                                     n.Observacion == "OBSERVACION_GENERAL");

            if (notaExistente != null)
            {
                notaExistente.Observacion = "OBSERVACION_GENERAL: " + observacion;
            }
            else
            {
                var nuevaNota = new NotaModels
                {
                    IdMatricula = idMatricula,
                    IdMateria = idMateria,
                    IdPeriodoAcademico = idPeriodoAcademico,
                    Observacion = "OBSERVACION_GENERAL: " + observacion
                };

                _context.NotaModels.Add(nuevaNota);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Observación guardada exitosamente en {Duration}ms",
                stopwatch.ElapsedMilliseconds); // ← NUEVA LÍNEA
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando observación para matrícula {IdMatricula}, materia {IdMateria}",
                idMatricula, idMateria); // ← NUEVA LÍNEA
            throw;
        }
    }

    // Resto de métodos con logging similar...
    public async Task<List<NotaModels>> ObtenerNotasEstudianteAsync(int idMatricula, int idMateria, int idPeriodoAcademico)
    {
        var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

        try
        {
            var result = await _context.NotaModels
                .Where(n => n.IdMatricula == idMatricula &&
                       n.IdMateria == idMateria &&
                       n.IdPeriodoAcademico == idPeriodoAcademico)
                .ToListAsync();

            _logger.LogInformation("Obtenidas {Count} notas para estudiante específico en {Duration}ms",
                result.Count, stopwatch.ElapsedMilliseconds); // ← NUEVA LÍNEA

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo notas específicas del estudiante"); // ← NUEVA LÍNEA
            throw;
        }
    }

    public async Task<List<MateriaConNotasDTO>> ObtenerNotasAgrupadas(int idMatricula, int idPeriodo)
    {
        var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

        try
        {
            _logger.LogInformation("Obteniendo notas agrupadas para matrícula {IdMatricula}, periodo {IdPeriodo}",
                idMatricula, idPeriodo); // ← NUEVA LÍNEA

            var resultado = new List<MateriaConNotasDTO>();
            var notas = await ObtenerNotasPorMatricula(idMatricula);

            var notasFiltradas = notas
                .Where(n => n.IdPeriodoAcademico == idPeriodo)
                .ToList();

            if (!notasFiltradas.Any())
            {
                _logger.LogInformation("No se encontraron notas para el periodo especificado"); // ← NUEVA LÍNEA
                return resultado;
            }

            var agrupadas = notasFiltradas
                .GroupBy(n => new { n.IdMateria, n.Materia.NomMateria });

            foreach (var grupo in agrupadas)
            {
                var dto = new MateriaConNotasDTO
                {
                    IdMateria = grupo.Key.IdMateria,
                    NombreMateria = grupo.Key.NomMateria,
                    Notas = grupo.Select(n => new NotaDTO
                    {
                        IdNota = n.IdNota,
                        NotaSaber = n.NotaSaber,
                        NotaHacer = n.NotaHacer,
                        NotaSer = n.NotaSer,
                        Observacion = n.Observacion
                    }).ToList()
                };

                resultado.Add(dto);
            }

            _logger.LogInformation("Notas agrupadas obtenidas exitosamente: {Count} materias en {Duration}ms",
                resultado.Count, stopwatch.ElapsedMilliseconds); // ← NUEVA LÍNEA

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo notas agrupadas para matrícula {IdMatricula}, periodo {IdPeriodo}",
                idMatricula, idPeriodo); // ← NUEVA LÍNEA
            throw;
        }
    }
}