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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                user.FailedLoginAttempts = failedAttempts;
                user.LockedUntil = lockedUntil;

                await _context.SaveChangesAsync();
            }
        }
    }
}
