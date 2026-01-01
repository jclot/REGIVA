using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.Exceptions;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.UI.Extensions;

namespace REGIVA_CR.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountLN _accountLN;
        private readonly ILogger<AccountController> _logger;

        private const string RegistrationSessionKey = "UserRegistrationStep1";

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
                if (ex.Message == "NOT_VERIFIED")
                {
                    _logger.LogInformation("Intento de login con cuenta no verificada: {Email}", model.Email);
                    string magicUrl = Url.Action("VerifyEmailLink", "Account", null, Request.Scheme)!;
                    await _accountLN.SendVerificationEmailAsync(model.Email!, magicUrl);

                    TempData["VerificationEmail"] = model.Email;
                    TempData["InfoMessage"] = "Tu cuenta aún no está verificada. Te hemos enviado un código nuevo.";

                    return RedirectToAction("VerifyEmail");
                }

                _logger.LogWarning(ex, "Error en Login: {Message}", ex.Message);
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Register(bool restart = false)
        {
            if (restart)
            {
                HttpContext.Session.Remove(RegistrationSessionKey);
                return RedirectToAction("Register");
            }

            UserRegisterDto? existingData = HttpContext.Session.GetObject<UserRegisterDto>(RegistrationSessionKey);

            if (existingData != null)
            {
                existingData.Password = string.Empty;
                existingData.ConfirmPassword = string.Empty;
                return View(existingData);
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

            HttpContext.Session.SetObject(RegistrationSessionKey, model);
            return RedirectToAction("RegisterTenant");
        }

        [HttpGet]
        public IActionResult RegisterTenant()
        {
            UserRegisterDto? userStep1 = HttpContext.Session.GetObject<UserRegisterDto>(RegistrationSessionKey);

            if (userStep1 == null)
            {
                return RedirectToAction("Register");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTenant(TenantRegisterDto tenantModel)
        {
            UserRegisterDto? userModel = HttpContext.Session.GetObject<UserRegisterDto>(RegistrationSessionKey);

            if (userModel == null)
            {
                return RedirectToAction("Register");
            }

            if (!ModelState.IsValid) return View(tenantModel);

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
                HttpContext.Session.Remove(RegistrationSessionKey);
                _logger.LogInformation("Usuario registrado (pendiente verificación): {Email}", userModel.Email);
                string magicUrl = Url.Action("VerifyEmailLink", "Account", null, Request.Scheme)!;
                await _accountLN.SendVerificationEmailAsync(userModel.Email!, magicUrl);
                TempData["VerificationEmail"] = userModel.Email;
                return RedirectToAction("VerifyEmail");
            }
            catch (DuplicateInfoException ex)
            {
                ModelState.AddModelError(ex.FieldName, ex.Message);
                _logger.LogWarning("Registro Tenant duplicado: {Message}", ex.Message);
                return View(tenantModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando Tenant completo");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la cuenta. Intente nuevamente.");
                return View(tenantModel);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            string? resetUrl = Url.Action("ResetPassword", "Account", null, Request.Scheme);
            if (resetUrl != null)
            {
                await _accountLN.SendPasswordResetAsync(model.Email!, resetUrl);
            }
            ViewData["SuccessMessage"] = "Si el correo existe, hemos enviado un enlace.";
            ModelState.Clear();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            UserRecoveryDto? userDto = await _accountLN.GetUserForResetAsync(token);

            if (userDto == null) return RedirectToAction("Login");

            return View(new ResetPasswordDto
            {
                Token = token,
                Email = email,
                TokenExpiration = userDto.ExpiresAt ?? DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            bool result = await _accountLN.ResetPasswordAsync(model.Token, model.Email, model.Password!);
            if (result)
            {
                ViewData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                return View("Login");
            }
            ViewData["ErrorMessage"] = "El enlace ha expirado o no es válido.";
            return View(model);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> VerifyEmail()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (TempData["VerificationEmail"] == null)
            {
                return RedirectToAction("Login");
            }

            string email = TempData["VerificationEmail"]!.ToString()!;
            bool isVerified = await _accountLN.IsEmailVerifiedAsync(email);

            if (isVerified)
            {
                TempData["SuccessMessage"] = "Tu cuenta ya fue verificada anteriormente. Por favor inicia sesión.";
                return RedirectToAction("Login");
            }

            VerifyEmailDto model = new VerifyEmailDto();
            model.Email = email;

            TempData.Keep("VerificationEmail");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto model)
        {
            if (!ModelState.IsValid) return View(model);

            bool result = await _accountLN.ConfirmEmailAsync(model.Email.Trim().ToLower(), model.Code.Trim());

            if (result)
            {
                ViewData["SuccessMessage"] = "¡Cuenta verificada! Ya puedes iniciar sesión.";
                return View("Login");
            }

            ModelState.AddModelError(string.Empty, "Código inválido o expirado.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmailLink(string email, string code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
                return RedirectToAction("Login");

            bool result = await _accountLN.ConfirmEmailAsync(email, code);

            if (result)
            {
                TempData["SuccessMessage"] = "Correo verificado exitosamente. Inicia sesión.";
                return RedirectToAction("Login");
            }

            TempData["VerificationEmail"] = email;
            TempData["ErrorMessage"] = "El enlace expiró. Solicita un nuevo código.";
            return RedirectToAction("VerifyEmail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            string magicUrl = Url.Action("VerifyEmailLink", "Account", null, Request.Scheme)!;
            await _accountLN.SendVerificationEmailAsync(email, magicUrl);

            TempData["VerificationEmail"] = email;
            TempData["SuccessMessage"] = "Nuevo código enviado.";
            return RedirectToAction("VerifyEmail");
        }
    }
}