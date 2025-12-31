using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace REGIVA_CR.UI.Controllers;

[Authorize]
public class ReportsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}