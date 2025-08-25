using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text.Json;

namespace AcademicoSFA.Pages.RegistroNotas
{
    public class UploadModel : PageModel
    {
        private readonly ILogger<UploadModel> _logger;
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;
        private readonly MateriaRepository _materiaRepository;
        private readonly MatriculaRepository _matriculaRepository;
        private readonly SfaDbContext _context;
        private readonly NotasRepository _notasRepository;

        public UploadModel(ILogger<UploadModel> logger, PeriodoAcademicoRepository periodoAcademicoRepository,
            MateriaRepository materiaRepository, MatriculaRepository matriculaRepository,
            SfaDbContext context, NotasRepository notasRepository)
        {
            _logger = logger;
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _materiaRepository = materiaRepository;
            _matriculaRepository = matriculaRepository;
            _context = context;
            _notasRepository = notasRepository;
        }

        [BindProperty]
        public IFormFile File { get; set; }

        public List<CursoNotas> CursosNotas { get; set; } = new List<CursoNotas>();
        public string DebugInfo { get; set; } = "";

        // Propiedades para mostrar información de carga
        public int TotalEstudiantesProcesados { get; set; } = 0;
        public int TotalMateriasProcesadas { get; set; } = 0;
        public int TotalNotasGuardadas { get; set; } = 0;
        public List<string> ErroresEncontrados { get; set; } = new List<string>();
        public List<NotificacionCarga> Notificaciones { get; set; } = new List<NotificacionCarga>();

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (File == null || File.Length == 0)
                {
                    ModelState.AddModelError("File", "Por favor, seleccione un archivo CSV válido.");
                    AddToast("Error", "No se ha seleccionado ningún archivo", "error");
                    return Page();
                }

                // Verificar la extensión del archivo
                var fileExtension = Path.GetExtension(File.FileName).ToLowerInvariant();
                if (fileExtension != ".csv")
                {
                    ModelState.AddModelError("File", "Solo se permiten archivos CSV.");
                    AddToast("Formato incorrecto", "El archivo debe tener formato CSV", "warning");
                    return Page();
                }

                AddDebug($"Procesando archivo: {File.FileName}, Tamaño: {File.Length} bytes");
                // Añadir notificaciones de progreso
                AddToast("Procesando archivo", $"Iniciando procesamiento de {File.FileName}", "info");

                // Para estadísticas
                int totalEstudiantesProcesados = 0;
                int totalMateriasProcesadas = 0;
                int totalNotasGuardadas = 0;

                using (var reader = new StreamReader(File.OpenReadStream(), System.Text.Encoding.GetEncoding("ISO-8859-1")))
                {
                    // Leer todo el contenido para procesarlo
                    string contenido = await reader.ReadToEndAsync();
                    string[] lineas = contenido.Split('\n');

                    AddDebug($"Total de líneas: {lineas.Length}");

                    // Variables para el procesamiento
                    string cursoActual = null;
                    string materiaActual = null;
                    string docenteActual = null;
                    int materiasEncontradas = 0;

                    // Lista para mantener solo información resumida de lo procesado
                    var resumenCursos = new List<ResumenCurso>();
                    ResumenCurso resumenActual = null;

                    CursoNotas cursoNotasActual = null;

                    // Recorrer todas las líneas
                    for (int i = 0; i < lineas.Length; i++)
                    {
                        string linea = lineas[i].Trim();

                        // Omitir líneas vacías
                        if (string.IsNullOrWhiteSpace(linea))
                            continue;

                        // Detectar línea de curso
                        if (linea.StartsWith("CURSO:;"))
                        {
                            var partes = linea.Split(';');
                            if (partes.Length > 1)
                            {
                                cursoActual = partes[1].Trim();
                                AddDebug($"Curso encontrado: {cursoActual}");
                            }
                            continue;
                        }

                        // Detectar línea de materia
                        if (linea.StartsWith("MATERIA:;"))
                        {
                            materiasEncontradas++;

                            var partes = linea.Split(';');
                            if (partes.Length > 2 && !string.IsNullOrWhiteSpace(partes[2]))
                            {
                                materiaActual = partes[2].Trim();
                                AddDebug($"Materia encontrada: {materiaActual} (#{materiasEncontradas})");
                            }
                            else
                            {
                                materiaActual = $"Materia sin nombre #{materiasEncontradas}";
                                AddDebug($"Materia sin nombre encontrada (#{materiasEncontradas})");
                            }

                            // Crear nuevo objeto de curso y notas
                            cursoNotasActual = new CursoNotas
                            {
                                Curso = cursoActual,
                                Materia = materiaActual,
                                Notas = new List<NotaEstudiante>()
                            };
                            CursosNotas.Add(cursoNotasActual);

                            // Crear nuevo resumen para esta materia
                            resumenActual = new ResumenCurso
                            {
                                Curso = cursoActual,
                                Materia = materiaActual,
                                TotalEstudiantes = 0
                            };

                            resumenCursos.Add(resumenActual);
                            totalMateriasProcesadas++;
                            continue;
                        }

                        // Detectar línea de docente
                        if (linea.StartsWith("DOCENTE:;"))
                        {
                            var partes = linea.Split(';');
                            if (partes.Length > 2)
                            {
                                docenteActual = partes[2].Trim();
                                if (resumenActual != null)
                                {
                                    resumenActual.Docente = docenteActual;
                                }
                                if (cursoNotasActual != null)
                                {
                                    cursoNotasActual.Docente = docenteActual;
                                }
                                AddDebug($"Docente encontrado: {docenteActual}");
                            }
                            continue;
                        }

                        // Detectar línea de encabezados 
                        if (linea.StartsWith("TAM;") && linea.Contains(";ESTUDIANTE;"))
                        {
                            string[] encabezados = linea.Split(';');
                            AddDebug($"Encabezados encontrados en línea {i}: {string.Join(", ", encabezados.Take(5))}...");

                            // Procesar todas las líneas siguientes que parezcan datos de estudiantes
                            i++; // Avanzar a la siguiente línea después de los encabezados

                            while (i < lineas.Length)
                            {
                                string lineaEstudiante = lineas[i].Trim();

                                // Si encontramos una nueva materia o línea vacía, terminamos con esta materia
                                if (lineaEstudiante.StartsWith("MATERIA:;") ||
                                    string.IsNullOrWhiteSpace(lineaEstudiante) ||
                                    lineaEstudiante.StartsWith(";;;;;"))
                                {
                                    i--; // Retroceder para que en la próxima iteración se detecte la nueva materia
                                    break;
                                }

                                var valoresEstudiante = lineaEstudiante.Split(';');

                                // Verificar que la línea tenga suficientes datos
                                if (valoresEstudiante.Length < 3)
                                {
                                    i++;
                                    continue;
                                }

                                // Verificar que tenga un TAM válido
                                if (!int.TryParse(valoresEstudiante[0], out int tam) || tam <= 0)
                                {
                                    // Si encontramos una línea que comienza con "0;0;" o "#N/A", podría ser el fin de los estudiantes
                                    if (lineaEstudiante.StartsWith("0;0;") || lineaEstudiante.StartsWith("#N/A;"))
                                    {
                                        break;
                                    }

                                    i++;
                                    continue;
                                }

                                // Crear una nueva nota de estudiante
                                var nota = new NotaEstudiante
                                {
                                    Tam = tam,
                                    Curso = cursoActual,
                                    Materia = materiaActual,
                                    Docente = docenteActual
                                };

                                // Agregar al cursoNotasActual
                                if (cursoNotasActual != null)
                                {
                                    cursoNotasActual.Notas.Add(nota);
                                }

                                totalEstudiantesProcesados++;
                                if (resumenActual != null)
                                {
                                    resumenActual.TotalEstudiantes++;
                                }

                                var idMateria = await _materiaRepository.ObtenerIdMateria(materiaActual);
                                var idMatricula = await _matriculaRepository.ObtenerIdMatricula(tam.ToString());
                                var idPeriodoAcademico = _periodoAcademicoRepository.ObtenerPeriodoAcademicoActual();

                                // Intentar obtener número de estudiante y nombre
                                if (valoresEstudiante.Length > 1)
                                {
                                    int.TryParse(valoresEstudiante[1], out int numEstudiante);
                                    nota.NumeroEstudiante = numEstudiante;
                                }

                                if (valoresEstudiante.Length > 2)
                                {
                                    nota.NombreEstudiante = valoresEstudiante[2].Trim();
                                }

                                // Variables para calcular promedios
                                decimal saberSer = 0;
                                List<decimal> notasSaberHacer = new List<decimal>();
                                List<decimal> notasSaberSaber = new List<decimal>();
                                decimal examenFinal = 0;

                                // Procesar el resto de valores según los encabezados
                                for (int k = 3; k < Math.Min(encabezados.Length, valoresEstudiante.Length); k++)
                                {
                                    string encabezado = encabezados[k].Trim();// Ejemplo autoevaluacion
                                    string valor = valoresEstudiante[k].Trim(); // valo 3.0  

                                    if (string.IsNullOrWhiteSpace(valor) || valor == "SN" || valor == "#¡REF!")
                                    {
                                        continue;
                                    }

                                    // Intentar convertir el valor a decimal
                                    decimal? valorDecimal = GetDecimalValue(valor);

                                    if (!valorDecimal.HasValue)
                                        continue;

                                    // Clasificar las notas según las dimensiones
                                    if (encabezado.Contains("HETEROEVALUAC"))
                                    {
                                        nota.Heteroevaluacion = valorDecimal;
                                        nota.Notas["SABER SER - HETEROEVALUACION"] = valorDecimal;
                                        await GuardarNotaEnBaseDeDatos(idMatricula, idMateria, idPeriodoAcademico, 0, 0, valorDecimal.Value, "SABER SER - HETEROEVALUACION");
                                    }
                                    else if (encabezado.Contains("AUTOEVALUACION"))
                                    {
                                        nota.Autoevaluacion = valorDecimal;
                                        nota.Notas["SABER SER - AUTOEVALUACION"] = valorDecimal;
                                        await GuardarNotaEnBaseDeDatos(idMatricula, idMateria, idPeriodoAcademico, 0, 0, valorDecimal.Value, "SABER SER - AUTOEVALUACION");
                                    }
                                    else if (encabezado.Contains("COEVALUACION"))
                                    {
                                        nota.Coevaluacion = valorDecimal;
                                        nota.Notas["SABER SER - COEVALUACION"] = valorDecimal;
                                        await GuardarNotaEnBaseDeDatos(idMatricula, idMateria, idPeriodoAcademico, 0, 0, valorDecimal.Value, "SABER SER - COEVALUACION");
                                    }
                                    else if (encabezado.Contains("SH"))
                                    //else if (encabezado.Contains("NOTA") && encabezado.Contains("SABER HACER"))
                                    {
                                        nota.Notas[encabezado] = valorDecimal;
                                        notasSaberHacer.Add(valorDecimal.Value);
                                        await GuardarNotaEnBaseDeDatos(idMatricula, idMateria, idPeriodoAcademico, 0, valorDecimal.Value, 0, "SABER HACER");
                                    }
                                    //else if (encabezado.Contains("NOTA") && encabezado.Contains("SABER SABER"))
                                    else if (encabezado.Contains("SS"))
                                    {
                                        nota.Notas[encabezado] = valorDecimal;
                                        notasSaberSaber.Add(valorDecimal.Value);
                                        await GuardarNotaEnBaseDeDatos(idMatricula, idMateria, idPeriodoAcademico, valorDecimal.Value, 0, 0, "SABER SABER");
                                    }
                                    else
                                    {
                                        nota.Notas[encabezado] = valorDecimal;
                                    }
                                }
                                i++;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                if (totalMateriasProcesadas > 0)
                {
                    AddToast("Materias encontradas", $"Se han identificado {totalMateriasProcesadas} materias en el archivo", "success");
                }

                // Al finalizar con éxito
                AddToast("Proceso completado", $"Se han guardado {TotalNotasGuardadas} notas exitosamente", "success");

                // Durante el procesamiento, actualiza los contadores
                TotalEstudiantesProcesados = totalEstudiantesProcesados;
                TotalMateriasProcesadas = totalMateriasProcesadas;

                // Agregar notificaciones de éxito para cada materia
                foreach (var curso in CursosNotas)
                {
                    Notificaciones.Add(new NotificacionCarga
                    {
                        Tipo = "success",
                        Mensaje = $"Se cargaron {curso.Notas.Count} estudiantes en {curso.Materia} ({curso.Curso})"
                    });
                }

                // Generar detalles por materia
                var detallesMaterias = new List<DetalleMateria>();
                foreach (var curso in CursosNotas)
                {
                    detallesMaterias.Add(new DetalleMateria
                    {
                        NombreMateria = curso.Materia,
                        Curso = curso.Curso,
                        Docente = curso.Docente,
                        CantidadEstudiantes = curso.Notas.Count,
                        NotasGuardadas = curso.Notas.Count * 3, // Aproximado, cada estudiante tiene 3 tipos de notas
                        Estado = "Completado" // Por defecto, podría ser "Parcial" si hay errores
                    });
                }

                // Guardar información en TempData para la página de resultados
                TempData["ResultadoCarga"] = JsonSerializer.Serialize(new ResultadoCarga
                {
                    TotalEstudiantes = totalEstudiantesProcesados,
                    TotalMaterias = totalMateriasProcesadas,
                    TotalNotas = TotalNotasGuardadas
                });

                if (ErroresEncontrados.Count > 0)
                {
                    TempData["ErroresCarga"] = JsonSerializer.Serialize(ErroresEncontrados);
                }

                TempData["DetallesCarga"] = JsonSerializer.Serialize(detallesMaterias);

                // Si hubieron algunos errores menores
                if (ErroresEncontrados.Count > 0 && ErroresEncontrados.Count < 5)
                {
                    AddToast("Advertencias", $"Se encontraron {ErroresEncontrados.Count} problemas menores", "warning");
                }

                // Redirigir a la página de resultados en lugar de quedarse en esta página
                return RedirectToPage("./ResultadoCargaNotas");
            }
            catch (Exception ex)
            {
                AddToast("Error crítico", "No se pudo procesar el archivo. Revise los detalles.", "error");
                AddDebug($"ERROR CRÍTICO: {ex.Message}\n{ex.StackTrace}");
                ErroresEncontrados.Add($"Error crítico: {ex.Message}");
                TempData["DebugInfo"] = DebugInfo;
                ModelState.AddModelError("", $"Error al procesar el archivo: {ex.Message}");
                return Page();
            }
        }

        private void AddToast(string title, string message, string type = "success")
        {
            // Si no hay múltiples toasts, inicializar la lista
            if (TempData["MultipleToasts"] == null)
            {
                TempData["MultipleToasts"] = JsonSerializer.Serialize(new List<ToastNotification>());
            }

            // Obtener la lista actual
            var toastsJson = TempData["MultipleToasts"].ToString();
            var toasts = JsonSerializer.Deserialize<List<ToastNotification>>(toastsJson);

            // Añadir la nueva notificación
            toasts.Add(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = type,
                Duration = type == "error" || type == "danger" ? 8000 : 5000 // Más tiempo para errores
            });

            // Actualizar TempData
            TempData["MultipleToasts"] = JsonSerializer.Serialize(toasts);
        }

        private class ResumenCurso
        {
            public string Curso { get; set; }
            public string Materia { get; set; }
            public string Docente { get; set; }
            public int TotalEstudiantes { get; set; }
            public List<string> Estudiantes { get; set; }
        }

        private async Task GuardarNotaEnBaseDeDatos(int idMatricula, int idMateria, int idPeriodoAcademico, decimal? valorSaber, decimal? valorHacer, decimal? valorSer, string observacion)
        {
            try
            {
                await _notasRepository.GuardarNotaAsync(
                    idMatricula,
                    idMateria,
                    idPeriodoAcademico,
                    valorSaber,
                    valorHacer,
                    valorSer,
                    observacion
                );

                TotalNotasGuardadas++;
            }
            catch (Exception ex)
            {
                ErroresEncontrados.Add($"Error al guardar nota de {observacion}: {ex.Message}");

                // Añadir toast solo para errores importantes o recurrentes
                if (ErroresEncontrados.Count == 1 || ErroresEncontrados.Count == 10 || ErroresEncontrados.Count == 50)
                {
                    AddToast("Error al guardar nota", $"Problema al guardar {observacion}. Se han encontrado {ErroresEncontrados.Count} errores.", "error");
                }

                throw;
            }
        }

        private void AddDebug(string message)
        {
            DebugInfo += message + "\n";
            _logger?.LogInformation(message);
        }

        private decimal? GetDecimalValue(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "SN" || value == "#¡REF!" || value == " ")
                return null;

            // Normalizar el formato decimal (reemplazar comas por puntos si es necesario)
            value = value.Replace(',', '.');

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return null;
        }
    }
}