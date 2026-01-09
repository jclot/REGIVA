using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AD.Entidades
{
    [Table("users")]
    public class UserEntity
    {
        [Key, Column("user_id")] public int UserId { get; set; }
        [Column("user_uuid")] public Guid UserUuid { get; set; } = Guid.NewGuid();
        [Column("email")][MaxLength(150)] public string? Email { get; set; }
        [Column("password_hash")][MaxLength(255)] public string? PasswordHash { get; set; }
        [Column("first_name")][MaxLength(100)] public string? FirstName { get; set; }
        [Column("last_name")][MaxLength(100)] public string? LastName { get; set; }
        [Column("phone")][MaxLength(20)] public string? Phone { get; set; }
        [Column("role")][MaxLength(30)] public string Role { get; set; } = "user";
        [Column("failed_login_attempts")] public int FailedLoginAttempts { get; set; } = 0;
        [Column("locked_until")] public DateTime? LockedUntil { get; set; }
        [Column("deleted_at")] public DateTime? DeletedAt { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("reset_token")][MaxLength(100)] public string? ResetToken { get; set; }
        [Column("reset_token_expires")] public DateTime? ResetTokenExpires { get; set; }
        [Column("is_email_verified")] public bool IsEmailVerified { get; set; } = false;
        [Column("verification_code")][MaxLength(6)] public string? VerificationCode { get; set; }
        [Column("verification_code_expires")] public DateTime? VerificationCodeExpires { get; set; }

    }

    [Table("tenants")]
    public class TenantEntity
    {
        [Key, Column("tenant_id")] public int TenantId { get; set; }
        [Column("tenant_uuid")] public Guid TenantUuid { get; set; } = Guid.NewGuid();
        [Column("business_name")][MaxLength(250)] public string? BusinessName { get; set; }
        [Column("legal_id")][MaxLength(20)] public string? LegalId { get; set; }
        [Column("email")][MaxLength(150)] public string? Email { get; set; }
        [Column("phone")][MaxLength(20)] public string? Phone { get; set; }
        [Column("economic_activity_code")][MaxLength(10)] public string? EconomicActivityCode { get; set; }
        [Column("subscription_plan")][MaxLength(50)] public string? SubscriptionPlan { get; set; }
        [Column("status")][MaxLength(20)] public string Status { get; set; } = "active";
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("tenant_users")]
    public class TenantUserEntity
    {
        [Key, Column("tenant_user_id")] public int TenantUserId { get; set; }
        [Column("tenant_id")] public int TenantId { get; set; }
        [Column("user_id")] public int UserId { get; set; }
        [Column("role_in_tenant")] public string? RoleInTenant { get; set; }
        [Column("is_active")] public bool IsActive { get; set; } = true;
    }

    [Table("blogs")]
    public class BlogEntity
    {
        [Key, Column("blog_id")] public int BlogId { get; set; }
        [Column("title")][MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Column("slug")][MaxLength(250)] public string Slug { get; set; } = string.Empty;
        [Column("summary")][MaxLength(500)] public string Summary { get; set; } = string.Empty;
        [Column("content_html")] public string ContentHtml { get; set; } = string.Empty;
        [Column("author_name")][MaxLength(100)] public string AuthorName { get; set; } = string.Empty;
        [Column("is_published")] public bool IsPublished { get; set; } = false;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    }

    [Table("activity_logs")]
    public class ActivityLogEntity
    {
        [Key, Column("log_id")] public long LogId { get; set; }
        [Column("tenant_id")] public int? TenantId { get; set; }
        [Column("user_id")] public int? UserId { get; set; }
        [Column("activity_type")][MaxLength(50)] public string ActivityType { get; set; } = string.Empty;
        [Column("action_description")] public string? ActionDescription { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("entity_type")] public string? EntityType { get; set; }
        [Column("entity_id")] public string? EntityId { get; set; }
        [Column("status")] public string Status { get; set; } = "success";

        [Column("ip_address", TypeName = "inet")]
        public IPAddress? IpAddress { get; set; }
    }

    [Table("password_history")]
    public class PasswordHistoryEntity
    {
        [Key, Column("history_id")]
        public long HistoryId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("password_hash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("user_invitations")]
    public class UserInvitationEntity
    {
        [Key, Column("invitation_id")] public long InvitationId { get; set; }
        [Column("tenant_id")] public int TenantId { get; set; }
        [Column("email")] public string Email { get; set; } = string.Empty;
        [Column("role_to_assign")] public string RoleToAssign { get; set; } = "user";
        [Column("token")] public string Token { get; set; } = string.Empty;
        [Column("expires_at")] public DateTime ExpiresAt { get; set; }
        [Column("status")] public string Status { get; set; } = "pending";
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
