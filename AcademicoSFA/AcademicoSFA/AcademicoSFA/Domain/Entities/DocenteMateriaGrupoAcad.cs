using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AcademicoSFA.Domain.Entities
{
    public class DocenteMateriaGrupoAcad
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Participante")]
        [Column("id_participante")]
        public int IdParticipante { get; set; }
        public Participante? Participante { get; set; } = default!; // Asocia con el modelo de Participante

        [ForeignKey("Materia")]
        [Column("id_materia")]
        public int IdMateria { get; set; }
        public Materia? Materia { get; set; } = default!; // Asocia con el modelo de Materia

        [ForeignKey("GrupoAcad")]
        [Column("id_grupo_acad")]
        public int IdGrupoAcad { get; set; }
        public GrupoAcad? GrupoAcad { get; set; } = default!; // Asocia con el modelo de GrupoAcad
    }
}
