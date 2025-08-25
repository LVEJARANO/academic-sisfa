namespace AcademicoSFA.Domain.Entities
{
    public class MatriculaInfo
    {
        public int IdMatricula { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Documento { get; set; }
        public string Email { get; set; }
        public string Activo { get; set; }
        public string Grado { get; set; }
        public int AnioEscolar { get; set; }

    }
}
