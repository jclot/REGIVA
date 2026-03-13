using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using REGIVA_CR.AB.AccesoADatos.Auth;
using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.AB.ModelosParaUI.Organization;
using REGIVA_CR.AD.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                                               where u.Email == email && u.DeletedAt == null && tu.IsActive
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
            return await _context.Users.AnyAsync(u => u.Email == email && u.DeletedAt == null);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone && u.DeletedAt == null);
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

        public async Task<bool> IsUserSuspendedAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            IQueryable<bool> query = from u in _context.Users
                                     join tu in _context.TenantUsers on u.UserId equals tu.UserId
                                     where u.Email == email && u.DeletedAt == null
                                     select tu.IsActive;

            bool hasActive = await query.AnyAsync(isActive => isActive);
            if (hasActive) return false;

            bool hasInactive = await query.AnyAsync(isActive => !isActive);
            return hasInactive;
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
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    await SavePasswordToHistoryAsync(userId, user.PasswordHash);
                }

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

        public async Task LogActivityAsync(int userId, int? tenantId, string type, string description, string? ipAddressString)
        {
            IPAddress? ip = null;

            if (!string.IsNullOrEmpty(ipAddressString) && IPAddress.TryParse(ipAddressString, out var parsedIp))
            {
                ip = parsedIp;
            }

            ActivityLogEntity log = new ActivityLogEntity
            {
                UserId = userId,
                TenantId = tenantId,
                ActivityType = type,
                ActionDescription = description,
                IpAddress = ip,
                CreatedAt = DateTime.UtcNow,
                Status = "success"
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
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

            UserProfileDto? profile = await query.FirstOrDefaultAsync();

            if (profile != null)
            {
                profile.RecentActivities = await _context.ActivityLogs
                    .Where(log => log.UserId == userId)
                    .OrderByDescending(log => log.CreatedAt)
                    .Take(10)
                    .Select(log => new UserActivityDto
                    {
                        Type = log.ActivityType,
                        Description = log.ActionDescription ?? "",
                        Timestamp = log.CreatedAt
                    })
                    .ToListAsync();
            }

            return profile;
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
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        await SavePasswordToHistoryAsync(model.UserId, user.PasswordHash);
                    }

                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task SavePasswordToHistoryAsync(int userId, string passwordHash)
        {
            PasswordHistoryEntity history = new PasswordHistoryEntity
            {
                UserId = userId,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };
            _context.PasswordHistory.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsPasswordInHistoryAsync(int userId, string newPassword, int limit = 4)
        {
            List<string> lastHashes = await _context.PasswordHistory
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(limit)
                .Select(h => h.PasswordHash)
                .ToListAsync();

            foreach (string hash in lastHashes)
            {
                if (BCrypt.Net.BCrypt.Verify(newPassword, hash))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<UserActivityDto>> GetActivityLogsAsync(int userId, int limit)
        {
            return await _context.ActivityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .Select(l => new UserActivityDto
                {
                    Type = l.ActivityType,
                    Description = l.ActionDescription ?? "",
                    Timestamp = l.CreatedAt
                })
                .ToListAsync();
        }

        public async Task SoftDeleteUserAsync(int userId)
        {
            UserEntity? user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.DeletedAt = DateTime.UtcNow;

                user.LockedUntil = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<OrganizationViewModel> GetOrganizationDetailsAsync(int tenantId)
        {
            TenantEntity? tenant = await _context.Tenants.FindAsync(tenantId);

            List<TeamMemberDto> members = await (from tu in _context.TenantUsers
                                                 join u in _context.Users on tu.UserId equals u.UserId
                                                 where tu.TenantId == tenantId && u.DeletedAt == null
                                                 select new TeamMemberDto
                                                 {
                                                     UserId = u.UserId,
                                                     FullName = u.FirstName + " " + u.LastName,
                                                     Email = u.Email ?? "",
                                                     Role = tu.RoleInTenant ?? "user",
                                                     IsActive = tu.IsActive,
                                                     JoinedAt = tu.JoinedAt
                                                 }).ToListAsync();

            List<PendingInviteViewDto> pendingInvites = await _context.Invitations
                .Where(i => i.TenantId == tenantId && i.Status == "pending")
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new PendingInviteViewDto
                {
                    Email = i.Email,
                    Role = i.RoleToAssign,
                    SentAt = i.CreatedAt
                }).ToListAsync();

            return new OrganizationViewModel
            {
                TenantId = tenantId,
                BusinessName = tenant?.BusinessName ?? "Empresa",
                Plan = tenant?.SubscriptionPlan ?? "Free",
                Members = members,
                PendingInvites = pendingInvites
            };
        }

        public async Task<TeamMemberDto?> GetTeamMemberAsync(int tenantId, int userId)
        {
            return await (from tu in _context.TenantUsers
                          join u in _context.Users on tu.UserId equals u.UserId
                          where tu.TenantId == tenantId && u.UserId == userId && u.DeletedAt == null
                          select new TeamMemberDto
                          {
                              UserId = u.UserId,
                              FullName = u.FirstName + " " + u.LastName,
                              Email = u.Email ?? "",
                              Role = tu.RoleInTenant ?? "user",
                              IsActive = tu.IsActive,
                              JoinedAt = tu.JoinedAt
                          }).FirstOrDefaultAsync();
        }

        public async Task UpdateTenantUserRoleAsync(int tenantId, int userId, string role)
        {
            TenantUserEntity? link = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);

            if (link != null)
            {
                link.RoleInTenant = role;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetTenantUserActiveAsync(int tenantId, int userId, bool isActive)
        {
            TenantUserEntity? link = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);

            if (link != null)
            {
                link.IsActive = isActive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTenantUserAsync(int tenantId, int userId)
        {
            TenantUserEntity? link = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);

            if (link != null)
            {
                _context.TenantUsers.Remove(link);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveInvitationAsync(InvitationDto dto)
        {
            UserInvitationEntity? existingInvite = await _context.Invitations
                .FirstOrDefaultAsync(i => i.TenantId == dto.TenantId && i.Email == dto.Email && i.Status == "pending");

            if (existingInvite != null)
            {
                existingInvite.Token = dto.Token;
                existingInvite.ExpiresAt = dto.ExpiresAt;
                existingInvite.RoleToAssign = dto.Role;
                existingInvite.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                UserInvitationEntity entity = new UserInvitationEntity
                {
                    TenantId = dto.TenantId,
                    Email = dto.Email,
                    RoleToAssign = dto.Role,
                    Token = dto.Token,
                    ExpiresAt = dto.ExpiresAt,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Invitations.Add(entity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<InvitationDto?> GetInvitationByTokenAsync(string token)
        {
            UserInvitationEntity? entity = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Token == token && i.Status == "pending" && i.ExpiresAt > DateTime.UtcNow);

            if (entity == null) return null;

            return new InvitationDto
            {
                TenantId = entity.TenantId,
                Email = entity.Email,
                Role = entity.RoleToAssign,
                Token = entity.Token,
                ExpiresAt = entity.ExpiresAt
            };
        }

        public async Task<int> CreateUserFromInviteAsync(AcceptInviteDto model, string passwordHash)
        {
            UserEntity? existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                if (existingUser.DeletedAt == null)
                {
                    throw new Exception($"El correo {model.Email} ya está activo en el sistema. Por favor inicia sesión.");
                }

                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.Phone = model.Phone;
                existingUser.PasswordHash = passwordHash;
                existingUser.IsEmailVerified = true;

                existingUser.DeletedAt = null;
                existingUser.LockedUntil = null;
                existingUser.FailedLoginAttempts = 0;

                existingUser.Role = "user";

                await _context.SaveChangesAsync();
                return existingUser.UserId;
            }
            else
            {
                UserEntity user = new UserEntity
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    PasswordHash = passwordHash,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    UserUuid = Guid.NewGuid(),
                    Role = "user"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user.UserId;
            }
        }

        public async Task LinkUserToTenantAsync(int userId, int tenantId, string role)
        {
            TenantUserEntity? existingLink = await _context.TenantUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(tu => tu.UserId == userId && tu.TenantId == tenantId);

            if (existingLink != null)
            {
                existingLink.RoleInTenant = role;
                existingLink.IsActive = true;
            }
            else
            {
                TenantUserEntity link = new TenantUserEntity
                {
                    UserId = userId,
                    TenantId = tenantId,
                    RoleInTenant = role,
                    IsActive = true
                };
                _context.TenantUsers.Add(link);
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkInvitationAsAcceptedAsync(string token)
        {
            UserInvitationEntity? invite = await _context.Invitations.FirstOrDefaultAsync(i => i.Token == token);
            if (invite != null)
            {
                invite.Status = "accepted";
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteInvitationAsync(int tenantId, string email)
        {
            UserInvitationEntity? invite = await _context.Invitations
                .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Email == email && i.Status == "pending");

            if (invite != null)
            {
                _context.Invitations.Remove(invite);
                await _context.SaveChangesAsync();
            }
        }
    }
}
