namespace AcademicoSFA.Domain.Entities
{
    // Clases compartidas para las páginas de registro de notas
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

    public class ResultadoCarga
    {
        public int TotalMaterias { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalNotas { get; set; }
    }

    public class NotificacionCarga
    {
        public string Tipo { get; set; } // "success", "warning", "danger", "info"
        public string Mensaje { get; set; }
    }
}
