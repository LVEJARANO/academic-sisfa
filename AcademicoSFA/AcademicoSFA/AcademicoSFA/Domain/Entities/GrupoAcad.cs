using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicoSFA.Domain.Entities
{
    public class GrupoAcad
    {
        [Key]
        [Column("id_grupo_acad")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Grupo requerido")]
        [DisplayName("Grupo")]
        [StringLength(30)]
        [Column("nom_grupo")]
        [RegularExpression("^[A-D]$", ErrorMessage = "El grupo debe ser A, B, C, D")]
        public required string NomGrupo { get; set; }
        [ForeignKey("Periodo")]
        [Column("id_periodo")]
        public required int IdPeriodo { get; set; }
        public Periodo Periodo { get; set; } = default!;

        [ForeignKey("Grado")]
        [Column("id_grado")]
        public required int IdGrado { get; set; }
        public Grado Grado { get; set; } = default!;  // Relación de navegación
    }
}
