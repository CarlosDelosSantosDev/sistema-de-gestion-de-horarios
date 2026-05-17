using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    public class Turno
    {
        public int IdGrupo { get; set; }
        public int Grado { get; set; }
        public string Seccion { get; set; }
        public int IdCarrera { get; set; }
        public int IdTurno { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        // Propiedades de navegación
        public string NombreCarrera { get; set; }
        public string NombreTurno { get; set; }

        public string NombreCompleto => string.IsNullOrEmpty(Seccion) ?
            $"{Grado}° {NombreCarrera}" :
            $"{Grado}° {Seccion} {NombreCarrera}";
    }
}
