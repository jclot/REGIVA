using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

            string? tenantId = User.FindFirst("TenantId")?.Value;
            string? role = User.FindFirst(ClaimTypes.Role)?.Value;
            string? email = User.FindFirst(ClaimTypes.Name)?.Value;

            List<Claim> newClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email ?? ""),
            new Claim("FullName", $"{model.FirstName} {model.LastName}"),
            new Claim("UserId", userId.ToString()),
            new Claim("TenantId", tenantId ?? ""),
            new Claim(ClaimTypes.Role, role ?? "")
        };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            int? tId = int.TryParse(tenantId, out var t) ? t : null;

            await _accountLN.LogActivityAsync(model.UserId, tId, "Perfil Actualizado", "Se modificaron datos personales o seguridad.", ipAddress);

            TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityLog()
    {
        int userId = int.Parse(User.FindFirst("UserId")!.Value);

        List<UserActivityDto> logs = await _accountLN.GetUserActivityLogsAsync(userId, 1000);

        return Json(new { data = logs });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string password)
    {
        try
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            await _accountLN.DeleteAccountAsync(userId, password);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            await _accountLN.LogActivityAsync(userId, null, "Cuenta Eliminada", "El usuario solicitó la eliminación (Soft Delete).", ip);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Tu cuenta ha sido eliminada correctamente.";
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Settings");
        }
    }
}