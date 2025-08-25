using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AcademicoSFA.Domain.Entities
{
    public class Materia
    {
        [Key]
        [Column("id_materia")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Campo requerido")]
        [DisplayName("Materia")]
        [StringLength(100, ErrorMessage = "El nombre de la materia no puede exceder los 100 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la materia debe tener al menos 3 caracteres.")]

        [Column("nom_materia")]
        public required string NomMateria { get; set; }
    }
}
