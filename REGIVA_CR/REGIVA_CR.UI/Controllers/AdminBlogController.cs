using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.LogicaDeNegocio.Blog;
using REGIVA_CR.AB.ModelosParaUI.Blog;

namespace REGIVA_CR.UI.Controllers
{
    [Authorize]
    public class AdminBlogController : Controller
    {
        private readonly IBlogLN _blogLN;
        private const string ALLOWED_EMAIL = "juliclot123@gmail.com"; 

        public AdminBlogController(IBlogLN blogLN)
        {
            _blogLN = blogLN;
        }

        private bool IsAuthorized()
        {
            string? email = User.FindFirst(ClaimTypes.Name)?.Value;
            return email != null && email.Equals(ALLOWED_EMAIL, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home"); 

            List<BlogDto> blogs = await _blogLN.GetAllAsync(includeDrafts: true);
            return View(blogs);
        }

        [HttpGet]
        public async Task<IActionResult> Form(int? id)
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            if (id.HasValue)
            {
                BlogDto? blog = await _blogLN.GetByIdAsync(id.Value);
                if (blog == null) return NotFound();
                return View(blog);
            }
            return View(new BlogDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(BlogDto model)
        {
            if (!IsAuthorized()) return Unauthorized();
            if (!ModelState.IsValid) return View("Form", model);

            await _blogLN.SaveAsync(model, "Julián Clot");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthorized()) return Unauthorized();
            await _blogLN.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
