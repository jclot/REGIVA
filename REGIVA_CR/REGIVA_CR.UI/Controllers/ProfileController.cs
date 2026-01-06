using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.ModelosParaUI.Auth;
using System.Security.Claims;

namespace REGIVA_CR.UI.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAccountLN _accountLN;

    public ProfileController(IAccountLN accountLN)
    {
        _accountLN = accountLN;
    }

    public async Task<IActionResult> Index()
    {
        Claim? userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return RedirectToAction("Login", "Account");

        int userId = int.Parse(userIdClaim.Value);

        UserProfileDto? profile = await _accountLN.GetUserProfileAsync(userId);

        if (profile == null) return NotFound();

        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        int userId = int.Parse(User.FindFirst("UserId")!.Value);
        UpdateProfileDto? model = await _accountLN.GetUserForEditAsync(userId);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UpdateProfileDto model)
    {
        if (!ModelState.IsValid) return View(model);

        int userId = int.Parse(User.FindFirst("UserId")!.Value);
        model.UserId = userId;

        try
        {
            await _accountLN.UpdateProfileAsync(model);
            TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}