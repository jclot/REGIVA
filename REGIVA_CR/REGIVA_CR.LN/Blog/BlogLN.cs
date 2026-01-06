using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using REGIVA_CR.AB.AccesoADatos.Blog;
using REGIVA_CR.AB.LogicaDeNegocio.Blog;
using REGIVA_CR.AB.ModelosParaUI.Blog;

namespace REGIVA_CR.LN.Blog
{
    public class BlogLN : IBlogLN
    {
        private readonly IBlogAD _blogAD;

        public BlogLN(IBlogAD blogAD)
        {
            _blogAD = blogAD;
        }

        public async Task<List<BlogDto>> GetAllAsync(bool includeDrafts)
        {
            return await _blogAD.GetAllBlogsAsync(!includeDrafts);
        }

        public async Task<BlogDto?> GetByIdAsync(int id)
        {
            return await _blogAD.GetBlogByIdAsync(id);
        }

        public async Task<BlogDto?> GetBySlugAsync(string slug)
        {
            return await _blogAD.GetBlogBySlugAsync(slug);
        }

        public async Task SaveAsync(BlogDto model, string authorName)
        {
            if (model.Id == 0)
            {
                model.Slug = GenerateSlug(model.Title);
                model.CreatedAt = DateTime.Now;
                model.Author = authorName;
                await _blogAD.CreateBlogAsync(model);
            }
            else
            {
                await _blogAD.UpdateBlogAsync(model);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _blogAD.DeleteBlogAsync(id);
        }

        public async Task<List<BlogDto>> SearchAsync(string query)
        {
            return await _blogAD.SearchBlogsAsync(query.ToLower().Trim());
        }

        private string GenerateSlug(string title)
        {
            string slug = title.ToLower().Trim();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim();
            return slug + "-" + DateTime.Now.Ticks.ToString().Substring(10);
        }
    }
}
