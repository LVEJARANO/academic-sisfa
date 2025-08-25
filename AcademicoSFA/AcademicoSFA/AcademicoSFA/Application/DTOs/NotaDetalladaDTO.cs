namespace AcademicoSFA.Application.DTOs
{
    public class NotaDetalladaDTO
    {
        public int IdNota { get; set; }
        public decimal? NotaSaber { get; set; }
        public decimal? NotaHacer { get; set; }
        public decimal? NotaSer { get; set; }
        public string Observacion { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
