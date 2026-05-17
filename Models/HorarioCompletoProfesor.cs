using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    public class HorarioCompletoProfesor
    {
        public int IdProfesor { get; set; }
        public string NombreProfesor { get; set; }
        public string Periodo { get; set; }
        public List<Horario> Clases { get; set; }  // Todas las clases del profesor

        public HorarioCompletoProfesor()
        {
            Clases = new List<Horario>();
        }
    }
}
