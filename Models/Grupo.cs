using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    public class Grupo
    {
        public int IdGrupo { get; set; }
        public int Grado { get; set; }
        public string Seccion { get; set; } // Debe permitir NULL
        public int IdCarrera { get; set; }
        public int IdTurno { get; set; }
        public string NombreCarrera { get; set; }
        public string NombreTurno { get; set; }
        public string ClaveCarrera { get; set; }
        public string EsDelPeriodo { get; set; }

        public string Periodo { get; set; }  // ✅ AGREGAR ESTA PROPIEDAD

        // ✅ NUEVA PROPIEDAD PARA MODALIDAD
        public string Modalidad { get; set; }

        // ✅ PROPERTY QUE OMITE SECCIÓN CUANDO ES NULL O VACÍA
        public string ClaveCompleta
        {
            get
            {
                var partes = new List<string>();

                if (!string.IsNullOrEmpty(ClaveCarrera))
                    partes.Add(ClaveCarrera);

                partes.Add(Grado.ToString());

                if (!string.IsNullOrWhiteSpace(Seccion))
                    partes.Add(Seccion);

                if (!string.IsNullOrEmpty(NombreTurno))
                {
                    string turnoAbreviado = NombreTurno.Length > 0 ?
                        NombreTurno.Substring(0, 1) : "";
                    if (!string.IsNullOrEmpty(turnoAbreviado))
                        partes.Add(turnoAbreviado);
                }

                // ✅ AGREGAR MODALIDAD ABREVIADA (EL o PR)
                if (!string.IsNullOrEmpty(Modalidad))
                {
                    string modalidadAbreviada = Modalidad.ToUpper() == "EN LÍNEA" ? "EL" : "PR";
                    partes.Add(modalidadAbreviada);
                }

                return string.Join(" ", partes);
            }
            set { }
        }

        // ✅ PROPERTY ADICIONAL para mostrar en ListBox (opcional)
        public string DisplayText
        {
            get
            {
                string textoBase = ClaveCompleta;
                if (!string.IsNullOrEmpty(EsDelPeriodo))
                {
                    textoBase = $"{EsDelPeriodo} {textoBase}";
                }
                return textoBase;
            }
        }
    }
}
