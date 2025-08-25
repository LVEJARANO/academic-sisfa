using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AcademicoSFA.Domain.Entities
{
    [Table("TBL_GRUPO_ACAD_MATERIA")]
    public class GrupoAcadMateria
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("GrupoAcad")]
        [Column("id_grupo_acad")]
        [Required]
        public int IdGrupoAcad { get; set; }
        public GrupoAcad? GrupoAcad { get; set; } = default!;

        [ForeignKey("Materia")]
        [Column("id_materia")]
        [Required]
        public int IdMateria { get; set; }
        public Materia? Materia { get; set; } = default!;
    }
}
