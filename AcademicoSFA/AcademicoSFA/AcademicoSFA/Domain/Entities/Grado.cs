using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicoSFA.Domain.Entities
{
    public class Grado
    {
        [Key]
        [Column("id_grado")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo requerido")]
        [DisplayName("Grado")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El grado solo puede contener números.")]
        [Column("nom_grado")]
        public required string NomGrado { get; set; }
    }
}
