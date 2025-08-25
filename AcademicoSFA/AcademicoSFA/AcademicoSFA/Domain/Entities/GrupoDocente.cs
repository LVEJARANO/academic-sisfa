using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AcademicoSFA.Domain.Entities
{
    [Table("TBL_GRUPO_DOCENTE")]  // Mapear el modelo a la tabla
    public class GrupoDocente
    {
        [Key]
        [Column("id_grupo_docente")]
        public int Id { get; set; }

        [ForeignKey("GrupoAcad")]
        [Column("id_grupo_acad")]
        public int IdGrupoAcad { get; set; }
        public GrupoAcad? GrupoAcad { get; set; } = default!;

        [ForeignKey("Participante")]
        [Column("id_participante")]
        public int IdParticipante { get; set; }
        public Participante? Participante { get; set; } = default!;


    }
}
