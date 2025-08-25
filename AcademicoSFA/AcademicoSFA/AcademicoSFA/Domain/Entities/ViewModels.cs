using System;
using System.Collections.Generic;

namespace AcademicoSFA.Domain.Entities
{
    public class MateriaNotasViewModel
    {
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; }
        public List<NotaViewModel> Notas { get; set; } = new List<NotaViewModel>();
        public decimal PromedioSaber { get; set; }
        public decimal PromedioHacer { get; set; }
        public decimal PromedioSer { get; set; }
        public decimal PromedioFinal { get; set; }
    }

    public class NotaViewModel
    {
        public int IdNota { get; set; }
        public decimal? NotaSaber { get; set; }
        public decimal? NotaHacer { get; set; }
        public decimal? NotaSer { get; set; }
        public string Observacion { get; set; }
        public decimal NotaFinal { get; set; }
    }

    public class EstudianteNotasMateriaViewModel
    {
        public int IdMatricula { get; set; }
        public string Codigo { get; set; }
        public string NombreCompleto { get; set; }
        public List<NotaViewModel> Notas { get; set; } = new List<NotaViewModel>();
        public decimal PromedioSaber { get; set; }
        public decimal PromedioHacer { get; set; }
        public decimal PromedioSer { get; set; }
        public decimal PromedioFinal { get; set; }
    }

    public class EstudianteConNotasViewModel
    {
        public int IdMatricula { get; set; }
        public string Codigo { get; set; }
        public string NombreCompleto { get; set; }
        public List<MateriaNotasViewModel> Materias { get; set; } = new List<MateriaNotasViewModel>();
        public decimal PromedioGeneral { get; set; }
    }
}