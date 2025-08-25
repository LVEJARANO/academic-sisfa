using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicoSFA.Domain.Entities
{
    public class NotaModels
    {
        [Key]
        [Column("id_nota")]
        public int IdNota { get; set; }
        [Column("id_matricula")]
        public int IdMatricula { get; set; }
        [Column("id_materia")]
        public int IdMateria { get; set; }
        [Column("id_periodo_academico")]
        public int IdPeriodoAcademico { get; set; }
        [Column("nota_saber")]
        public decimal? NotaSaber { get; set; }
        [Column("nota_hacer")]
        public decimal? NotaHacer { get; set; }
        [Column("nota_ser")]
        public decimal? NotaSer { get; set; }
        [Column("observacion")]
        public string Observacion { get; set; }

        // Propiedades de navegación
        public virtual Materia Materia { get; set; }
        public virtual MatriculaModels Matricula { get; set; }
        public virtual PeriodoAcademico PeriodoAcademico { get; set; }
    }
}
