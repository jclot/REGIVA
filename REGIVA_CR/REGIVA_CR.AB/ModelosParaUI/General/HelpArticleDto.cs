using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace REGIVA_CR.AB.ModelosParaUI.General
{
    public class HelpArticleDto
    {
        public string Id { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public string IconClass { get; set; } = "bi-file-text"; 
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
