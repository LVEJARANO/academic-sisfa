using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AcademicoSFA.Domain.Entities
{

    [Table("tbl_periodo_academico")]
    public class PeriodoAcademico
    {
        [Key]
        [Column("id_periodo_academico")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El año lectivo es obligatorio")]
        [Column("id_periodo")]
        [Display(Name = "Año Lectivo")]
        public int IdPeriodo { get; set; }

        [Required(ErrorMessage = "El número de periodo es obligatorio")]
        [Column("numero_periodo")]
        [Display(Name = "Número de Periodo")]
        [Range(1, 10, ErrorMessage = "El número de periodo debe estar entre 1 y 10")]
        public int NumeroPeriodo { get; set; }

        [Required(ErrorMessage = "El nombre del periodo es obligatorio")]
        [Column("nombre")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre del Periodo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Column("fecha_inicio")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [Column("fecha_fin")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }

        [ForeignKey("IdPeriodo")]
        public virtual Periodo Periodo { get; set; }
    }

}
