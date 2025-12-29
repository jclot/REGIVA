using Microsoft.AspNetCore.Mvc;

namespace REGIVA_CR.UI.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
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

        public IActionResult Terms() => View();
        public IActionResult Privacy() => View();
    }
}
