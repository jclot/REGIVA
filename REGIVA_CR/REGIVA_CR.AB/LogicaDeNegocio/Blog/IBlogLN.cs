using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REGIVA_CR.AB.ModelosParaUI.Blog;

namespace REGIVA_CR.AB.LogicaDeNegocio.Blog
{
    public interface IBlogLN
    {
        Task<List<BlogDto>> GetAllAsync(bool includeDrafts);
        Task<BlogDto?> GetByIdAsync(int id);
        Task<BlogDto?> GetBySlugAsync(string slug);
        Task SaveAsync(BlogDto model, string authorName);
        Task DeleteAsync(int id);
        Task<List<BlogDto>> SearchAsync(string query);
    }
}
