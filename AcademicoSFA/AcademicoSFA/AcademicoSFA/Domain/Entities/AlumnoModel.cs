using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace AcademicoSFA.Domain.Entities
{
    [Table("TBL_ALUMNO")]  // Mapear el modelo a la tabla
    public class AlumnoModel
    {
        [Key]
        [Column("codigo")]
        [Required(ErrorMessage = "Campo obligatorio.")]
        [DisplayName("TAM")]
        public required string Codigo { get; set; }
        [ForeignKey("Participante")]
        [Column("id_participante")]
        public int IdParticipante { get; set; }
        public Participante Participante { get; set; } = default!;
        [Required(ErrorMessage = "Campo obligatorio.")]
        [DisplayName("Estado")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El estado solo pueden contener letras.")]
        [Column("estado")]
        public required string Estado { get; set; }

        // Agregar una lista de matrículas relacionadas con el alumno
        public ICollection<MatriculaModels> Matriculas { get; set; } = new List<MatriculaModels>();

    }
}
