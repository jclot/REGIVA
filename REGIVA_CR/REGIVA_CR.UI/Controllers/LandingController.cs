using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.AB.ModelosParaUI.General;
using REGIVA_CR.AB.Services;
using REGIVA_CR.LN.Services;

namespace REGIVA_CR.UI.Controllers
{
    public class LandingController : Controller
    {

        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IDocsService _docsService;

        public LandingController(IEmailService emailService, IConfiguration config, IDocsService docsService)
        {
            _emailService = emailService;
            _config = config;
            _docsService = docsService;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        public IActionResult Pricing()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Docs(string? q)
        {
            List<HelpArticleDto> articles = string.IsNullOrEmpty(q)
                ? _docsService.GetFeaturedArticles()
                : _docsService.SearchArticles(q);


            ViewData["SearchQuery"] = q;
            return View(articles);
        }

        [HttpGet]
        [Route("Docs/Article/{id}")]
        public IActionResult Article(string id)
        {
            HelpArticleDto? article = _docsService.GetArticleById(id);
            if (article == null) return RedirectToAction("Docs");

            return View(article);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactDto model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                string adminEmail = _config["EmailSettings:Email"]!;
                string body = $@"
                    <div style='font-family: sans-serif; border: 1px solid #ddd; padding: 20px; border-radius: 8px;'>
                        <h2 style='color: #206bc4;'>Nuevo Mensaje de Contacto Web</h2>
                        <p>Alguien ha llenado el formulario de contacto en REGIVA:</p>
                        <hr>
                        <p><strong>Nombre:</strong> {model.Name}</p>
                        <p><strong>Correo del Cliente:</strong> <a href='mailto:{model.Email}'>{model.Email}</a></p>
                        <p><strong>Teléfono:</strong> {model.Phone}</p>
                        <p><strong>Asunto:</strong> {model.Subject}</p>
                        <br>
                        <p><strong>Mensaje:</strong></p>
                        <blockquote style='background: #f9f9f9; padding: 15px; border-left: 4px solid #206bc4;'>
                            {model.Message.Replace("\n", "<br>")}
                        </blockquote>
                    </div>";

                await _emailService.SendEmailAsync(adminEmail, $"[Contacto Web] {model.Subject} - {model.Name}", body);
                TempData["SuccessMessage"] = "¡Gracias! Hemos recibido tu mensaje. Te contactaremos pronto.";

                return RedirectToAction("Contact");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Hubo un error al enviar el mensaje. Intenta de nuevo.");
                return View(model);
            }
        }


        public IActionResult Status() => View();
        public IActionResult Security() => View();

        public IActionResult Terms() => View();
        public IActionResult Privacy() => View();
    }
}
