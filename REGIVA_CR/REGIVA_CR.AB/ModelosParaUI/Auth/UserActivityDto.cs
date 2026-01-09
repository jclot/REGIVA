using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.ModelosParaUI.Auth
{
    public class UserActivityDto
    {
        public string Type { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public string ColorClass
        {
            get
            {
                string t = Type.ToLower();
                if (t.Contains("login") || t.Contains("sesión")) return "bg-green-lt";
                if (t.Contains("crear") || t.Contains("nuevo")) return "bg-azure-lt";
                if (t.Contains("eliminar") || t.Contains("borrar")) return "bg-red-lt";
                return "bg-blue-lt";
            }
        }
    }
}
