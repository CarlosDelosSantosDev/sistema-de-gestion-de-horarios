using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    public class Profesor
    {
        public int NumeroTrabajador { get; set; }
        public string NombreDocente { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string NombreCompleto => $"{NombreDocente} {ApellidoPaterno} {ApellidoMaterno}";
    }
}
