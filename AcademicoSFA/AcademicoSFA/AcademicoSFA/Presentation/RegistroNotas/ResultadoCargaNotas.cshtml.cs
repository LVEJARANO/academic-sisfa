using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;

namespace AcademicoSFA.Pages.RegistroNotas
{
    public class ResultadoCargaModel : PageModel
    {
        public int TotalMaterias { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalNotas { get; set; }
        public List<DetalleMateria> DetallesMaterias { get; set; } = new List<DetalleMateria>();
        public List<string> Errores { get; set; } = new List<string>();

        public IActionResult OnGet()
        {
            // Recuperar datos de TempData 
            var resultadoJSON = TempData["ResultadoCarga"] as string;
            var erroresJSON = TempData["ErroresCarga"] as string;
            var detallesJSON = TempData["DetallesCarga"] as string;

            if (string.IsNullOrEmpty(resultadoJSON))
            {
                // Si no hay datos, redirigir a la página de carga
                AddToast("Información", "No hay resultados de carga disponibles", "info");
                return RedirectToPage("./Upload");
            }

            try
            {
                // Deserializar los resultados
                var resultado = JsonSerializer.Deserialize<ResultadoCarga>(resultadoJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TotalMaterias = resultado.TotalMaterias;
                TotalEstudiantes = resultado.TotalEstudiantes;
                TotalNotas = resultado.TotalNotas;

                // Deserializar errores si existen
                if (!string.IsNullOrEmpty(erroresJSON))
                {
                    Errores = JsonSerializer.Deserialize<List<string>>(erroresJSON);

                    if (Errores.Count > 0)
                    {
                        AddToast("Atención", $"Se encontraron {Errores.Count} errores durante la carga", "warning");
                    }
                }

                // Deserializar detalles si existen
                if (!string.IsNullOrEmpty(detallesJSON))
                {
                    DetallesMaterias = JsonSerializer.Deserialize<List<DetalleMateria>>(detallesJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Mostrar estadísticas por materia
                    if (DetallesMaterias.Count > 0)
                    {
                        var materiasCompletas = DetallesMaterias.Count(m => m.Estado == "Completado");

                        if (materiasCompletas == DetallesMaterias.Count)
                        {
                            AddToast("Materias procesadas", "Todas las materias fueron procesadas correctamente", "success");
                        }
                        else
                        {
                            var materiasIncompletas = DetallesMaterias.Count - materiasCompletas;
                            AddToast("Materias con problemas", $"{materiasIncompletas} materias no se procesaron completamente", "warning");
                        }
                    }
                }

                // Mostrar mensaje de éxito general
                AddToast("Carga exitosa", $"Se han guardado {TotalNotas} notas en total", "success");

                return Page();
            }
            catch (JsonException ex)
            {
                // Si hay error en la deserialización, redirigir a la página de carga
                AddToast("Error", "No se pudieron recuperar los datos de la carga: " + ex.Message, "error");
                return RedirectToPage("./Upload");
            }
        }

        // Método para añadir notificaciones toast
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
    }

    public class ResultadoCarga
    {
        public int TotalMaterias { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalNotas { get; set; }
    }

    public class DetalleMateria
    {
        public string NombreMateria { get; set; }
        public string Curso { get; set; }
        public string Docente { get; set; }
        public int CantidadEstudiantes { get; set; }
        public int NotasGuardadas { get; set; }
        public string Estado { get; set; } // "Completado", "Parcial", "Error"
    }

    public class ToastNotification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "success";
        public int Duration { get; set; } = 5000;
    }
}