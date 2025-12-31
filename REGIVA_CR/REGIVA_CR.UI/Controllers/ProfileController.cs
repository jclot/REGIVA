using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace REGIVA_CR.UI.Controllers;

[Authorize]
public class ProfileController : Controller
{
    // Ver Perfil
    public IActionResult Index()
    {
        return View();
    }

    // Configuración
    public IActionResult Settings()
    {
        return View();
    }
}