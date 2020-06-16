using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System;

namespace MoodyBudgeter.Data.Auth
{
    public class AuthContext : DbContext
    {
        public virtual DbSet<AuthApp> App { get; set; }
        public virtual DbSet<AuthSecurityRole> SecurityRole { get; set; }
        public virtual DbSet<AuthUserCredential> UserCredential { get; set; }
        public virtual DbSet<AuthUserSecurityRole> UserSecurityRole { get; set; }
        public virtual DbSet<AuthUserLoginHistory> UserLoginHistory { get; set; }

        public AuthContext()
        {
        }

        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DBConnection"));
                optionsBuilder.UseSqlServer("Data Source=WINDOWS-PCCIMCK;Initial Catalog=MoodyBudgeter; Integrated Security=false;user id=alyssa;password=alyssaTest;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthApp>(entity =>
            {
                entity.HasKey(e => e.ClientId);

                entity.ToTable("auth_App");

                entity.Property(e => e.ClientId)
                    .HasMaxLength(100)
                    .ValueGeneratedNever();

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<AuthSecurityRole>(entity =>
            {
                entity.HasKey(e => e.SecurityRoleId);

                entity.ToTable("auth_SecurityRole");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<AuthUserCredential>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("auth_UserCredential");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.FirstAttemptDate).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ResetExpiration).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<AuthUserSecurityRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.SecurityRoleId });

                entity.ToTable("auth_UserSecurityRole");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.SecurityRole)
                    .WithMany(p => p.UserSecurityRole)
                    .HasForeignKey(d => d.SecurityRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_auth_UserSecurityRole_SecurityRole");

                entity.HasOne(d => d.UserCredential)
                    .WithMany(p => p.UserSecurityRole)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_auth_UserSecurityRole_UserCredential");
            });

            modelBuilder.Entity<AuthUserLoginHistory>(entity =>
            {
                entity.HasKey(e => e.UserLoginHistoryId);

                entity.ToTable("auth_UserLoginHistory");

                entity.HasIndex(e => new { e.UserId, e.LoginDate })
                    .HasName("UserLoginHistory_Index_UserId_LoginDate");

                entity.Property(e => e.Audience).HasMaxLength(250);

                entity.Property(e => e.LoginDate).HasColumnType("datetime");

                entity.Property(e => e.Provider).HasMaxLength(200);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });
        }
    }
}
