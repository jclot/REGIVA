using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using REGIVA_CR.AB.AccesoADatos.Auth;
using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.AD.Entidades;

namespace REGIVA_CR.AD.Auth
{
    public class AccountAD : IAccountAD
    {
        private readonly RegivaContext _context;

        public AccountAD(RegivaContext context)
        {
            _context = context;
        }

        public async Task<int> RegisterUserAndTenantAsync(FullRegistrationDto data, string passwordHash)
        {
            IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                UserEntity user = new UserEntity
                {
                    Email = data.User?.Email,
                    Phone = data.User?.Phone,
                    FirstName = data.User?.FirstName,
                    LastName = data.User?.LastName,
                    PasswordHash = passwordHash,
                    Role = "admin"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TenantEntity tenant = new TenantEntity
                {
                    BusinessName = data.Tenant?.BusinessName,
                    LegalId = data.Tenant?.LegalId,

                    Email = data.User?.Email,

                    Phone = data.Tenant?.Phone,
                    EconomicActivityCode = data.Tenant?.EconomicActivityCode,
                    SubscriptionPlan = data.Tenant?.SubscriptionPlan
                };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                TenantUserEntity relation = new TenantUserEntity
                {
                    UserId = user.UserId,
                    TenantId = tenant.TenantId,
                    RoleInTenant = "owner"
                };
                _context.TenantUsers.Add(relation);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return user.UserId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserSessionDto?> GetUserByEmailAsync(string email)
        {
            IQueryable<UserSessionDto> query = from u in _context.Users
                                               join tu in _context.TenantUsers on u.UserId equals tu.UserId
                                               join t in _context.Tenants on tu.TenantId equals t.TenantId
                                               where u.Email == email
                                               select new UserSessionDto
                                               {
                                                   UserId = u.UserId,
                                                   TenantId = t.TenantId,
                                                   Email = u.Email,
                                                   FullName = $"{u.FirstName} {u.LastName}",
                                                   Role = tu.RoleInTenant,
                                                   BusinessName = t.BusinessName
                                               };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<string?> GetPasswordHashAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .Select(u => u.PasswordHash)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        public async Task<bool> TenantExistsAsync(string legalId)
        {
            return await _context.Tenants.AnyAsync(t => t.LegalId == legalId);
        }

        public async Task<UserSecurityDto?> GetUserSecurityInfoAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new UserSecurityDto
                {
                    UserId = u.UserId,
                    PasswordHash = u.PasswordHash,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    LockedUntil = u.LockedUntil
                })
                .FirstOrDefaultAsync();
        }

        public async Task UpdateUserLoginStatsAsync(int userId, int failedAttempts, DateTime? lockedUntil)
        {
            UserEntity? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                user.FailedLoginAttempts = failedAttempts;
                user.LockedUntil = lockedUntil;

                await _context.SaveChangesAsync();
            }
        }

        public async Task SavePasswordResetTokenAsync(string email, string token)
        {
            UserEntity? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                user.ResetToken = token;
                user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserRecoveryDto?> GetUserByResetTokenAsync(string token)
        {
            return await _context.Users
                .Where(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow)
                .Select(u => new UserRecoveryDto
                {
                    UserId = u.UserId,
                    Email = u.Email!,
                    ExpiresAt = u.ResetTokenExpires
                })
                .FirstOrDefaultAsync();
        }

        public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            UserEntity? user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PasswordHash = newPasswordHash;
                user.ResetToken = null;
                user.ResetTokenExpires = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetVerificationCodeAsync(string email, string code)
        {
            UserEntity? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.VerificationCode = code;
                user.VerificationCodeExpires = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            UserEntity? user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.VerificationCode == code &&
                u.VerificationCodeExpires > DateTime.UtcNow);

            if (user != null)
            {
                user.IsEmailVerified = true;
                user.VerificationCode = null;
                user.VerificationCodeExpires = null;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .Select(u => u.IsEmailVerified)
                .FirstOrDefaultAsync();
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            IQueryable<UserProfileDto> query = from u in _context.Users
                                               join tu in _context.TenantUsers on u.UserId equals tu.UserId
                                               join t in _context.Tenants on tu.TenantId equals t.TenantId
                                               where u.UserId == userId
                                               select new UserProfileDto
                                               {
                                                   UserId = u.UserId,
                                                   FullName = $"{u.FirstName} {u.LastName}",
                                                   Email = u.Email ?? "",
                                                   Phone = u.Phone ?? "No registrado",
                                                   Role = tu.RoleInTenant ?? "User",
                                                   MemberSince = u.CreatedAt,

                                                   BusinessName = t.BusinessName ?? "Sin empresa",
                                                   LegalId = t.LegalId ?? "",
                                                   EconomicActivity = t.EconomicActivityCode ?? "N/A",
                                                   Plan = t.SubscriptionPlan ?? "Free"
                                               };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<UpdateProfileDto?> GetUserForEditAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new UpdateProfileDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    Phone = u.Phone
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ValidateCurrentPasswordAsync(int userId, string currentPassword)
        {
            string? hash = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.PasswordHash)
                .FirstOrDefaultAsync();

            if (hash == null) return false;
            return BCrypt.Net.BCrypt.Verify(currentPassword, hash);
        }

        public async Task UpdateUserProfileAsync(UpdateProfileDto model)
        {
            UserEntity? user = await _context.Users.FindAsync(model.UserId);
            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
