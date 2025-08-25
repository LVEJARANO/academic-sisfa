using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicoSFA.Domain.Entities
{
    public class Periodo
    {
        [Key]
        [Column("id_periodo")]
        public int Id { get; set; }

        [Required]
        [Column("anio_escolar")]
        public int AnioEscolar { get; set; }

        [Required]
        [StringLength(2)]
        [Column("activo")]
        public required string Activo { get; set; }

        //[Required]
        //[Column("fecha_periodo1")]
        //public DateOnly FechaNota1 { get; set; }
        //[Required]
        //[Column("fecha_periodo2")]
        //public DateOnly FechaNota2 { get; set; }
        //[Required]
        //[Column("fecha_periodo3")]
        //public DateOnly FechaNota3 { get; set; }
        //[Required]
        //[Column("fecha_periodo4")]
        //public DateOnly FechaNota4 { get; set; }
    }
}
