using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.Exceptions;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.ModelosParaUI.Auth;

namespace REGIVA_CR.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountLN _accountLN;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountLN accountLN, ILogger<AccountController> logger)
        {
            _accountLN = accountLN;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Email = model.Email?.Trim().ToLower();

            try
            {
                UserSessionDto? user = await _accountLN.LoginAsync(model);

                if (user != null)
                {
                    _logger.LogInformation("Usuario {Email} inició sesión exitosamente.", user.Email);

                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
                        new Claim("FullName", user.FullName ?? string.Empty),
                        new Claim("TenantId", user.TenantId.ToString()),
                        new Claim("UserId", user.UserId.ToString()),
                        new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    AuthenticationProperties authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        AllowRefresh = true
                    };

                    if (model.RememberMe)
                    {
                        authProperties.ExpiresUtc = DateTime.UtcNow.AddDays(30);
                    }

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }

                _logger.LogWarning("Intento de login fallido para {Email}.", model.Email);
                ViewData["ErrorMessage"] = "Credenciales inválidas.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Login bloqueado o error para {Email}", model.Email);
                ViewData["ErrorMessage"] = ex.Message;
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario cerró sesión.");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (TempData["UserRegisterData"] is string userJson)
            {
                UserRegisterDto? model = JsonSerializer.Deserialize<UserRegisterDto>(userJson);
                TempData.Keep("UserRegisterData");
                return View(model);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(UserRegisterDto model)
        {
            if (!ModelState.IsValid) return View("Register", model);

            model.FirstName = model.FirstName?.Trim();
            model.LastName = model.LastName?.Trim();
            model.Email = model.Email?.Trim().ToLower();
            model.Phone = model.Phone?.Trim();

            try
            {
                await _accountLN.ValidateUserAvailabilityAsync(model.Email!, model.Phone!);
            }
            catch (DuplicateInfoException ex)
            {
                ModelState.AddModelError(ex.FieldName, ex.Message);
                _logger.LogWarning("Intento de registro duplicado: {Message}", ex.Message);
                return View("Register", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en RegisterUser");
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado. Intente más tarde.");
                return View("Register", model);
            }

            TempData["UserRegisterData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("RegisterTenant");
        }

        [HttpGet]
        public IActionResult RegisterTenant()
        {
            if (TempData["UserRegisterData"] == null)
            {
                return RedirectToAction("Register");
            }

            TempData.Keep("UserRegisterData");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTenant(TenantRegisterDto tenantModel)
        {
            string? userJson = TempData["UserRegisterData"] as string;

            if (string.IsNullOrEmpty(userJson)) return RedirectToAction("Register");

            TempData.Keep("UserRegisterData");

            if (!ModelState.IsValid) return View(tenantModel);

            UserRegisterDto? userModel = JsonSerializer.Deserialize<UserRegisterDto>(userJson);

            if (userModel == null)
            {
                return RedirectToAction("Register");
            }

            tenantModel.BusinessName = tenantModel.BusinessName?.Trim();
            tenantModel.LegalId = tenantModel.LegalId?.Trim();
            tenantModel.Phone = tenantModel.Phone?.Trim();
            tenantModel.EconomicActivityCode = tenantModel.EconomicActivityCode?.Trim();

            FullRegistrationDto fullRegistration = new FullRegistrationDto
            {
                User = userModel,
                Tenant = tenantModel
            };
            fullRegistration.Tenant.SubscriptionPlan = "basic";

            try
            {
                await _accountLN.ValidateTenantAvailabilityAsync(tenantModel.LegalId!);
                await _accountLN.RegisterAsync(fullRegistration);
                TempData.Remove("UserRegisterData");
                _logger.LogInformation("Nuevo Tenant registrado: {BusinessName}", tenantModel.BusinessName);
                ViewData["SuccessMessage"] = "Cuenta creada exitosamente. Por favor inicia sesión.";
                return View("Login");
            }
            catch (DuplicateInfoException ex)
            {
                ModelState.AddModelError(ex.FieldName, ex.Message);
                _logger.LogWarning("Registro Tenant duplicado: {Message}", ex.Message);
                return View(tenantModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico registrando Tenant {LegalId}", tenantModel.LegalId);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(tenantModel);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            _logger.LogInformation("Solicitud de recuperación de contraseña para: {Email}", model.Email);
            model.Email = model.Email?.Trim().ToLower();
            // await _accountLN.SendPasswordResetAsync(model.Email);

            ViewData["SuccessMessage"] = "Si el correo existe, hemos enviado un enlace.";
            ModelState.Clear();
            return View();
        }
    }
}