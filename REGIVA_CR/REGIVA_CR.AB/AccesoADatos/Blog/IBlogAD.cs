using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REGIVA_CR.AB.ModelosParaUI.Blog;

namespace REGIVA_CR.AB.AccesoADatos.Blog
{
    public interface IBlogAD
    {
        Task<List<BlogDto>> GetAllBlogsAsync(bool onlyPublished);
        Task<BlogDto?> GetBlogByIdAsync(int id);
        Task<BlogDto?> GetBlogBySlugAsync(string slug);
        Task CreateBlogAsync(BlogDto blogDto);
        Task UpdateBlogAsync(BlogDto blogDto);
        Task DeleteBlogAsync(int id);
        Task<List<BlogDto>> SearchBlogsAsync(string query);
    }
}
