using Microsoft.AspNetCore.Mvc;

namespace REGIVA_CR.UI.Controllers;
public class UsersController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}