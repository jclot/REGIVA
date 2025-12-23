using Microsoft.AspNetCore.Mvc;

namespace REGIVA_CR.UI.Controllers;
public class ReportsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}