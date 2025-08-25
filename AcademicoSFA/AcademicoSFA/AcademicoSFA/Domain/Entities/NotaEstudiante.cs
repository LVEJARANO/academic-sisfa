using System.Text.Json.Serialization;

namespace AcademicoSFA.Domain.Entities
{
    public class NotaEstudiante
    {
        [JsonPropertyName("curso")]
        public string Curso { get; set; }

        [JsonPropertyName("materia")]
        public string Materia { get; set; }

        [JsonPropertyName("docente")]
        public string Docente { get; set; }

        [JsonPropertyName("tam")]
        public int Tam { get; set; }

        [JsonPropertyName("numeroEstudiante")]
        public int NumeroEstudiante { get; set; }

        [JsonPropertyName("nombreEstudiante")]
        public string NombreEstudiante { get; set; }

        [JsonPropertyName("heteroevaluacion")]
        public decimal? Heteroevaluacion { get; set; }

        [JsonPropertyName("autoevaluacion")]
        public decimal? Autoevaluacion { get; set; }

        [JsonPropertyName("coevaluacion")]
        public decimal? Coevaluacion { get; set; }

        [JsonPropertyName("definitiva")]
        public decimal? Definitiva { get; set; }

        [JsonPropertyName("notas")]
        public Dictionary<string, decimal?> Notas { get; set; } = new Dictionary<string, decimal?>();

        [JsonPropertyName("desempeno")]
        public string Desempeno { get; set; }

        [JsonPropertyName("inasistencias")]
        public int Inasistencias { get; set; }
    }
}
