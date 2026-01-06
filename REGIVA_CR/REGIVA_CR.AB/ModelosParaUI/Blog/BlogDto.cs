using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.ModelosParaUI.Blog
{
    public class BlogDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El resumen es obligatorio")]
        [Display(Name = "Resumen Corto")]
        [StringLength(300, ErrorMessage = "Máximo 300 caracteres")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [Display(Name = "Contenido")]
        public string ContentHtml { get; set; } = string.Empty;

        [Display(Name = "Publicar ahora")]
        public bool IsPublished { get; set; }

        public string Slug { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
