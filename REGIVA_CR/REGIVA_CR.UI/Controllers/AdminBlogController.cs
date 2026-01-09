using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.LogicaDeNegocio.Blog;
using REGIVA_CR.AB.ModelosParaUI.Blog;
using REGIVA_CR.LN.Auth;
using System.Security.Claims;

namespace REGIVA_CR.UI.Controllers
{
    [Authorize]
    public class AdminBlogController : Controller
    {
        private readonly IBlogLN _blogLN;
        private const string ALLOWED_EMAIL = "juliclot123@gmail.com";
        private readonly IAccountLN _accountLN;

        public AdminBlogController(IBlogLN blogLN, IAccountLN accountLN)
        {
            _blogLN = blogLN;
            _accountLN = accountLN;
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


            string authorName = User.FindFirst("FullName")?.Value ?? "REGIVA";

            await _blogLN.SaveAsync(model, authorName);
            int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            string accion = model.Id == 0 ? "Blog Creado" : "Blog Editado";
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _accountLN.LogActivityAsync(userId, null, accion, $"Artículo: {model.Title}", ipAddress);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthorized()) return Unauthorized();

            BlogDto? blog = await _blogLN.GetByIdAsync(id);
            string titulo = blog?.Title ?? "Desconocido";

            await _blogLN.DeleteAsync(id);
            int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _accountLN.LogActivityAsync(userId, null, "Blog Eliminado", $"ID: {id}", ipAddress);

            return RedirectToAction("Index");
        }
    }
}
