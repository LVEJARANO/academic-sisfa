using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace AcademicoSFA.Domain.Entities
{
    [Table("TBL_PAGO")]  // Mapear el modelo a la tabla correspondiente
    public class PagoModel
    {
        [Key]
        [Column("id_pago")]
        public int Id { get; set; }

        [ForeignKey("Matricula")]
        [Required]
        [Column("id_matricula")]
        public int MatriculaId { get; set; }
        public MatriculaModels? Matricula { get; set; } = default!;

        [Required(ErrorMessage = "El tipo de pago es obligatorio")]
        [Column("tipo_pago")]
        [StringLength(50)]
        public string TipoPago { get; set; } = default!;  // Puede ser 'MENSUALIDAD' o 'MATRICULA'

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [Column("fecha_pago")]
        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Column("monto")]
        [Range(0, 1000000, ErrorMessage = "El monto debe ser un valor positivo")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El estado del pago es obligatorio")]
        [Column("estado_pago")]
        [StringLength(20)]
        public string EstadoPago { get; set; } = default!;  // Puede ser 'PAGADO' o 'PENDIENTE'

        [Column("mes_pagado")]
        [StringLength(20)]
        public string? MesPagado { get; set; }  // Solo se usa si es mensualidad

        [NotMapped] // Esta propiedad no se mapea a la base de datos
        public string MontoFormateado => Monto.ToString("F2", CultureInfo.InvariantCulture);
    }
}
