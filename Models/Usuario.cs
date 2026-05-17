using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHorarios.Models
{
    internal class Usuario
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string TipoUsuario { get; set; } // "admin" o "maestro"
        public DateTime FechaCreacion { get; set; }
    }
}

