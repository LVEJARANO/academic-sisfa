using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicoSFA.Domain.Entities
{
    [Table("TBL_PARTICIPANTE")]  // Mapear el modelo a la tabla
    public class Participante
    {
        [Key]
        [Column("id_participante")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Campo obligatorio.")]
        [DisplayName("Nombres")]
        [StringLength(250, ErrorMessage = "El nombre no puede exceder los 250 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Los nombres solo pueden contener letras.")]
        [Column("nombres")]  // Mapear la columna "nombres"
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        [DisplayName("Apellidos")]
        [StringLength(250, ErrorMessage = "El apellido no puede exceder los 250 caracteres.")]
        [MinLength(3, ErrorMessage = "El apellido debe tener al menos 3 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Los apellidos solo pueden contener letras.")]
        [Column("apellidos")]  // Mapear la columna "apellido"
        public required string Apellido { get; set; }
        [DisplayName("Número de identificación")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El número de identificación solo puede contener dígitos.")]
        [Column("documento")]  // Mapear la columna "documento"
        public string? Documento { get; set; }
        [Column("clave")]  // Mapear la columna "clave"
        [StringLength(20, ErrorMessage = "La contraseña no puede exceder los 20 caracteres.")]
        [MinLength(5, ErrorMessage = "La contraseña debe tener al menos 5 caracteres.")]
        [DataType(DataType.Password)]
        public string? Clave { get; set; }
        [Column("email")]  // Mapear la columna "email"
        [Required(ErrorMessage = "Campo obligatorio.")]
        [EmailAddress(ErrorMessage = "Por favor, ingrese una dirección de correo electrónico válida.")]
        [DisplayName("Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Column("Rol")]  // Mapear la columna "Rol"
        public string? Rol { get; set; }
        [Column("eliminado")]  
        public bool? Eliminado { get; set; }
    }
}
