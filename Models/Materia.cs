using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    public class Materia
    {
        public int IdMateria { get; set; }
        public string Nombre { get; set; }
        public string Grupo { get; set; }
        public string Clave { get; set; }
        public int? HorasSemana { get; set; }

        public string NombreCompleto => string.IsNullOrEmpty(Grupo) ?
            $"{Nombre} ({Clave})" :
            $"{Nombre} - {Grupo} ({Clave})";

        // Para mostrar en el ListView
        public override string ToString() => NombreCompleto;

        public double Horas => 1.0;
    }
}
