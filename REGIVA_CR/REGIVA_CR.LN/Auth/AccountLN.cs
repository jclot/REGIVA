using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using REGIVA_CR.AB.AccesoADatos.Auth;
using REGIVA_CR.AB.Exceptions;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.AB.Services;
using REGIVA_CR.LN.Services;

namespace REGIVA_CR.LN.Auth
{
    public class AccountLN : IAccountLN
    {
        private readonly IAccountAD _accountAD;
        private readonly IEmailService _emailService;

        public AccountLN(IAccountAD accountAD, IEmailService emailService)
        {
            _accountAD = accountAD;
            _emailService = emailService;
        }

        public async Task ValidateUserAvailabilityAsync(string email, string phone)
        {
            if (await _accountAD.UserExistsAsync(email))
            {
                throw new DuplicateInfoException("Email", $"El correo {email} ya se encuentra registrado.");
            }

            if (await _accountAD.PhoneExistsAsync(phone))
            {
                throw new DuplicateInfoException("Phone", $"El teléfono {phone} ya está asociado a otra cuenta.");
            }
        }

        public async Task ValidateTenantAvailabilityAsync(string legalId)
        {
            if (await _accountAD.TenantExistsAsync(legalId))
            {
                throw new DuplicateInfoException("LegalId", $"La cédula jurídica {legalId} ya está registrada.");
            }
        }

        public async Task RegisterAsync(FullRegistrationDto data)
        {
            if (data.User?.Email == null) throw new ArgumentNullException("Email");
            if (data.User?.Phone == null) throw new ArgumentNullException("Phone");
            if (data.Tenant?.LegalId == null) throw new ArgumentNullException("LegalId");

            await ValidateUserAvailabilityAsync(data.User.Email, data.User.Phone);
            await ValidateTenantAvailabilityAsync(data.Tenant.LegalId);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(data.User.Password!);
            await _accountAD.RegisterUserAndTenantAsync(data, passwordHash);
        }

        public async Task<UserSessionDto?> LoginAsync(LoginDto data)
        {
            if (string.IsNullOrEmpty(data.Email)) return null;

            bool isVerified = await _accountAD.IsEmailVerifiedAsync(data.Email);
            if (!isVerified)
            {
                bool exists = await _accountAD.UserExistsAsync(data.Email);
                if (exists)
                {
                    throw new Exception("NOT_VERIFIED");
                }
            }

            UserSecurityDto? securityInfo = await _accountAD.GetUserSecurityInfoAsync(data.Email);

            if (securityInfo == null) return null;

            if (securityInfo.LockedUntil.HasValue)
            {
                if (securityInfo.LockedUntil.Value > DateTime.UtcNow)
                {
                    TimeSpan timeRemaining = securityInfo.LockedUntil.Value - DateTime.UtcNow;
                    throw new Exception($"Cuenta bloqueada por seguridad. Intenta de nuevo en {Math.Ceiling(timeRemaining.TotalMinutes)} minutos.");
                }
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(data.Password, securityInfo.PasswordHash);

            if (isPasswordValid)
            {
                if (securityInfo.FailedLoginAttempts > 0 || securityInfo.LockedUntil != null)
                {
                    await _accountAD.UpdateUserLoginStatsAsync(securityInfo.UserId, 0, null);
                }
                return await _accountAD.GetUserByEmailAsync(data.Email);
            }
            else
            {
                int currentFails = securityInfo.FailedLoginAttempts;
                if (securityInfo.LockedUntil.HasValue && securityInfo.LockedUntil.Value < DateTime.UtcNow)
                {
                    currentFails = 0;
                }

                int newFailCount = currentFails + 1;
                DateTime? newLockDate = null;

                if (newFailCount >= 5)
                {
                    newLockDate = DateTime.UtcNow.AddMinutes(15);
                }

                await _accountAD.UpdateUserLoginStatsAsync(securityInfo.UserId, newFailCount, newLockDate);

                return null;
            }
        }

        public async Task SendPasswordResetAsync(string email, string resetLinkBaseUrl)
        {
            bool exists = await _accountAD.UserExistsAsync(email);
            if (!exists) return;
            string token = Guid.NewGuid().ToString();

            await _accountAD.SavePasswordResetTokenAsync(email, token);

            string link = $"{resetLinkBaseUrl}?token={token}&email={email}";

            string body = $@"
                <h3>Recuperación de Contraseña</h3>
                <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
                <a href='{link}'>Restablecer Contraseña</a>
                <p>Si no fuiste tú, ignora este mensaje.</p>";
            await _emailService.SendEmailAsync(email, "Restablecer Contraseña - REGIVA", body);
        }

        public async Task<bool> ResetPasswordAsync(string token, string email, string newPassword)
        {
            UserRecoveryDto? user = await _accountAD.GetUserByResetTokenAsync(token);

            if (user == null || user.Email != email) return false;

            string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _accountAD.UpdatePasswordAsync(user.UserId, newHash);

            return true;
        }

        public async Task<UserRecoveryDto?> GetUserForResetAsync(string token)
        {
            return await _accountAD.GetUserByResetTokenAsync(token);
        }

        public async Task SendVerificationEmailAsync(string email, string verificationLinkBaseUrl)
        {
            Random random = new Random();
            string code = random.Next(100000, 999999).ToString();

            await _accountAD.SetVerificationCodeAsync(email, code);

            string magicLink = $"{verificationLinkBaseUrl}?email={email}&code={code}";

            string body = $@"
        <div style='font-family:sans-serif; padding:20px; max-width:600px;'>
            <h2>Verifica tu correo</h2>
            <p>Gracias por registrarte en REGIVA. Para activar tu cuenta, usa el siguiente enlace:</p>
            
            <div style='margin: 30px 0;'>
                <a href='{magicLink}' style='background-color:#206bc4; color:white; padding:12px 24px; text-decoration:none; border-radius:4px; font-weight:bold; display:inline-block;'>
                    Verificar mi cuenta ahora
                </a>
            </div>

            <p>Si el botón no funciona, ingresa este código manualmente:</p>
            <div style='background:#f0f2f5; padding:15px; font-size:24px; letter-spacing:5px; font-weight:bold; text-align:center; border-radius:4px;'>
                {code}
            </div>
            
            <p style='color:gray; font-size:12px; margin-top:30px;'>Este código expira en 15 minutos.</p>
        </div>";

            await _emailService.SendEmailAsync(email, "Verifica tu cuenta - REGIVA", body);
        }

        public async Task<bool> ConfirmEmailAsync(string email, string code)
        {
            return await _accountAD.VerifyEmailAsync(email, code);
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            return await _accountAD.IsEmailVerifiedAsync(email);
        }
    }
}
