using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REGIVA_CR.AD.Entidades;

namespace REGIVA_CR.AD
{
    public class RegivaContext : DbContext
    {
        public RegivaContext(DbContextOptions<RegivaContext> options) : base(options) { }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<TenantUserEntity> TenantUsers { get; set; }
        public DbSet<BlogEntity> Blogs { get; set; }
        public DbSet<ActivityLogEntity> ActivityLogs { get; set; }
        public DbSet<PasswordHistoryEntity> PasswordHistory { get; set; }
        public DbSet<UserInvitationEntity> Invitations { get; set; }
    }
}
