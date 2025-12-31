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

namespace REGIVA_CR.LN.Auth
{
    public class AccountLN : IAccountLN
    {
        private readonly IAccountAD _accountAD;

        public AccountLN(IAccountAD accountAD)
        {
            _accountAD = accountAD;
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
    }
}
