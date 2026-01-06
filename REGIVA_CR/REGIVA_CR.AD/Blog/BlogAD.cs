using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REGIVA_CR.AB.AccesoADatos.Blog;
using REGIVA_CR.AB.ModelosParaUI.Blog;
using REGIVA_CR.AD.Entidades;

namespace REGIVA_CR.AD.Blog
{
    public class BlogAD : IBlogAD
    {
        private readonly RegivaContext _context;

        public BlogAD(RegivaContext context)
        {
            _context = context;
        }

        public async Task<List<BlogDto>> GetAllBlogsAsync(bool onlyPublished)
        {
            IQueryable<BlogEntity> query = _context.Blogs.AsQueryable();
            if (onlyPublished) query = query.Where(b => b.IsPublished);

            return await query.OrderByDescending(b => b.CreatedAt)
                .Select(e => new BlogDto
                {
                    Id = e.BlogId,
                    Title = e.Title,
                    Slug = e.Slug,
                    Summary = e.Summary,
                    ContentHtml = e.ContentHtml,
                    Author = e.AuthorName,
                    IsPublished = e.IsPublished,
                    CreatedAt = e.CreatedAt
                }).ToListAsync();
        }

        public async Task<BlogDto?> GetBlogByIdAsync(int id)
        {
            BlogEntity? e = await _context.Blogs.FindAsync(id);
            if (e == null) return null;

            return new BlogDto
            {
                Id = e.BlogId,
                Title = e.Title,
                Slug = e.Slug,
                Summary = e.Summary,
                ContentHtml = e.ContentHtml,
                Author = e.AuthorName,
                IsPublished = e.IsPublished,
                CreatedAt = e.CreatedAt
            };
        }

        public async Task<BlogDto?> GetBlogBySlugAsync(string slug)
        {
            BlogEntity? e = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
            if (e == null) return null;

            return new BlogDto
            {
                Id = e.BlogId,
                Title = e.Title,
                Slug = e.Slug,
                Summary = e.Summary,
                ContentHtml = e.ContentHtml,
                Author = e.AuthorName,
                IsPublished = e.IsPublished,
                CreatedAt = e.CreatedAt
            };
        }

        public async Task CreateBlogAsync(BlogDto dto)
        {
            BlogEntity entity = new BlogEntity
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Summary = dto.Summary,
                ContentHtml = dto.ContentHtml,
                AuthorName = dto.Author,
                IsPublished = dto.IsPublished,
                CreatedAt = dto.CreatedAt
            };
            _context.Blogs.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBlogAsync(BlogDto dto)
        {
            BlogEntity? entity = await _context.Blogs.FindAsync(dto.Id);
            if (entity != null)
            {
                entity.Title = dto.Title;
                entity.Summary = dto.Summary;
                entity.ContentHtml = dto.ContentHtml;
                entity.AuthorName = dto.Author;
                entity.IsPublished = dto.IsPublished;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBlogAsync(int id)
        {
            BlogEntity? entity = await _context.Blogs.FindAsync(id);
            if (entity != null)
            {
                _context.Blogs.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BlogDto>> SearchBlogsAsync(string query)
        {
            query = query?.Trim() ?? "";

            return await _context.Blogs
                .Where(b => b.IsPublished && (
                    b.Title.ToLower().Contains(query) ||
                    b.Summary.ToLower().Contains(query) ||

                    EF.Functions.ILike(
                        EF.Functions.Unaccent(b.AuthorName),
                        "%" + EF.Functions.Unaccent(query) + "%"
                    )
                ))
                .OrderByDescending(b => b.CreatedAt)
                .Select(e => new BlogDto
                {
                    Id = e.BlogId,
                    Title = e.Title,
                    Slug = e.Slug,
                    Summary = e.Summary,
                    ContentHtml = e.ContentHtml,
                    Author = e.AuthorName,
                    IsPublished = e.IsPublished,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();
        }
    }
}
