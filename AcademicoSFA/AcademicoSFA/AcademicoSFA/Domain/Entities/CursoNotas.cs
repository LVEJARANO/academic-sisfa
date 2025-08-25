using AcademicoSFA.Pages.RegistroNotas;
using System.Text.Json.Serialization;

namespace AcademicoSFA.Domain.Entities
{
    public class CursoNotas
    {
        [JsonPropertyName("curso")]
        public string Curso { get; set; }

        [JsonPropertyName("materia")]
        public string Materia { get; set; }

        [JsonPropertyName("docente")]
        public string Docente { get; set; }

        [JsonPropertyName("notas")]
        public List<NotaEstudiante> Notas { get; set; } = new List<NotaEstudiante>();
    }
}
