using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    public class Horario
    {
        public int IdHorario { get; set; }
        public int IdProfesor { get; set; }
        public int IdMateria { get; set; }
        public int IdGrupo { get; set; }
        public string DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Periodo { get; set; }
        public bool EsNP { get; set; }

        // Propiedades para mostrar
        public string TextoMostrado { get; set; }
        public bool EsMateria { get; set; }

        // Propiedades de navegación
        public string NombreProfesor { get; set; }
        public string NombreMateria { get; set; }
        public string ClaveMateria { get; set; }
        public string ClaveCarrera { get; set; }
        public int Grado { get; set; }
        public string Seccion { get; set; }
        public string Turno { get; set; }
        public string TipoElemento { get; set; }

        // ✅ CAMBIO IMPORTANTE: Hacer la propiedad Horas editable
        public double Horas { get; set; }

        // Propiedad adicional para saber si es turno nocturno
        public bool EsNocturno { get; set; }

        public string Modalidad { get; set; }  // ✅ AGREGAR ESTA PROPIEDAD
    }
}

