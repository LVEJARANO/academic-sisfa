using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicoSFA.Domain.Entities
{
    [Table("TBL_MATRICULA")]  // Mapear el modelo a la tabla
    public class MatriculaModels
    {
        [Key]
        [Column("id_matricula")]
        public required int IdMatricula { get; set; }
        [Required]
        [StringLength(30)]
        [ForeignKey("Alumno")] 
        [Column("codigo")]
        public required string Codigo { get; set; }
        public AlumnoModel Alumno { get; set; } = default!;
        [Required]
        [Column("id_periodo")]
        public required int IdPeriodo { get; set; }
        [ForeignKey("IdPeriodo")]
        public Periodo Periodos { get; set; } = default!;

        [StringLength(2)]
        [Column("activo")]
        [DisplayName("Estado")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El estado solo pueden contener letras.")]
        [Required(ErrorMessage = "Estado requerido")]
        public required string Activo { get; set; }

        [Required]
        [Column("id_grupo_acad")]
        public required int IdGrupoAcad { get; set; }
        [ForeignKey("IdGrupoAcad")]
        public GrupoAcad GruposAcad { get; set; } = default!;
        [Column("pago_al_dia")]
        public bool PagoAlDia { get; set; }



        // Relación con los pagos
        public ICollection<PagoModel> Pagos { get; set; } = new List<PagoModel>();
    }
}
