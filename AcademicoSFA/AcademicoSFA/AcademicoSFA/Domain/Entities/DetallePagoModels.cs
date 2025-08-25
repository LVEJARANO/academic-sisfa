namespace AcademicoSFA.Domain.Entities
{
    public class DetallePagoModels
    {
        public string TAM { get; set; }
        public string nombre { get; set; }
        public string? documento { get; set; }
        public string email { get; set; }
        public string mes_pagado { get; set; }
        public decimal monto { get; set; }
        public DateTime fecha_pago { get; set; }
        public string tipo_pago { get; set; }
        public int id_matricula { get; set; }
        public int id_pago { get; set; }
    }
}
