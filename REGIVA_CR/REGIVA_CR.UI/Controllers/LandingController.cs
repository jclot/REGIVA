using Microsoft.AspNetCore.Mvc;

namespace REGIVA_CR.UI.Controllers
{
    public class LandingController : Controller
    {
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

        public IActionResult Docs()
        {
            return View();
        }

        public IActionResult Contact() => View();
        public IActionResult Status() => View();
        public IActionResult Security() => View();

        public IActionResult Terms() => View();
        public IActionResult Privacy() => View();
    }
}
