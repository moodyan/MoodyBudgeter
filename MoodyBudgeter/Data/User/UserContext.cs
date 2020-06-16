using Microsoft.EntityFrameworkCore;
using System;

namespace MoodyBudgeter.Data.User
{
    public class UserContext : DbContext
    {
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<ProfilePropertyDefinition> ProfilePropertyDefinition { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }

        public UserContext() { }

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

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
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasIndex(e => e.Email)
                    .HasName("IX_Users_LowerEmail");

                entity.HasIndex(e => e.Username);

                entity.HasIndex(e => new { e.UserId, e.IsSuperUser, e.LastModifiedOnDate })
                    .HasName("IX_Users_LastModifiedOnDate");

                entity.HasIndex(e => new { e.UserId, e.IsSuperUser, e.Email, e.IsDeleted, e.DisplayName })
                    .HasName("IX_Users_IsDeleted_DisplayName");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserID");

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastIpaddress)
                    .HasColumnName("LastIPAddress")
                    .HasMaxLength(50);

                entity.Property(e => e.LastModifiedByUserId).HasColumnName("LastModifiedByUserID");

                entity.Property(e => e.LastModifiedOnDate).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LowerEmail)
                    .HasMaxLength(256)
                    .HasComputedColumnSql("(lower([Email]))");

                entity.Property(e => e.PasswordResetExpiration).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.ProfileId);

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserProfile");

                entity.HasIndex(e => new { e.UserId, e.LastUpdatedDate })
                    .HasName("IX_UserProfile_LastUpdatedDate");

                entity.HasIndex(e => new { e.UserId, e.PropertyDefinitionId });

                entity.HasIndex(e => new { e.ProfileId, e.UserId, e.PropertyValue, e.PropertyDefinitionId })
                    .HasName("IX_UserProfile_PropertyDefinitionID");

                entity.Property(e => e.ProfileId).HasColumnName("ProfileID");

                entity.Property(e => e.ExtendedVisibility)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.PropertyDefinitionId).HasColumnName("PropertyDefinitionID");

                entity.Property(e => e.PropertyText).HasColumnType("nvarchar(max)");

                entity.Property(e => e.PropertyValue).HasMaxLength(3750);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Visibility).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.PropertyDefinition)
                    .WithMany(p => p.UserProfile)
                    .HasForeignKey(d => d.PropertyDefinitionId)
                    .HasConstraintName("FK_UserProfile_ProfilePropertyDefinition");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserProfile)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserProfile_Users");
            });

            modelBuilder.Entity<ProfilePropertyDefinition>(entity =>
            {
                entity.HasKey(e => e.PropertyDefinitionId);

                entity.HasIndex(e => e.PropertyName);

                entity.HasIndex(e => new { e.ModuleDefId, e.PropertyName })
                    .HasName("IX_ProfilePropertyDefinition")
                    .IsUnique();

                entity.Property(e => e.PropertyDefinitionId).HasColumnName("PropertyDefinitionID");

                entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserID");

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.DataTypeEnum).HasDefaultValueSql("((0))");

                entity.Property(e => e.DefaultValue).HasColumnType("ntext");

                entity.Property(e => e.InputMask).HasMaxLength(250);

                entity.Property(e => e.JsRegex).HasMaxLength(100);

                entity.Property(e => e.Label).HasMaxLength(250);

                entity.Property(e => e.LastModifiedByUserId).HasColumnName("LastModifiedByUserID");

                entity.Property(e => e.LastModifiedOnDate).HasColumnType("datetime");

                entity.Property(e => e.Length).HasDefaultValueSql("((0))");

                entity.Property(e => e.ModuleDefId).HasColumnName("ModuleDefID");

                entity.Property(e => e.PcreRegex).HasMaxLength(100);

                entity.Property(e => e.PropertyCategory)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PropertyName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ValidationExpression).HasMaxLength(2000);
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);

                entity.HasIndex(e => new { e.RoleId, e.UserId })
                    .HasName("IX_UserRoles_RoleUser")
                    .IsUnique();

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .HasName("IX_UserRoles_UserRole")
                    .IsUnique();

                entity.HasIndex(e => new { e.UserId, e.RoleId, e.UserRoleId })
                    .HasName("_dta_index_UserRoles_6_1045578763__K2_K3_K1");

                entity.HasIndex(e => new { e.UserRoleId, e.UserId, e.ExpiryDate, e.IsTrialUsed, e.EffectiveDate, e.CreatedByUserId, e.CreatedOnDate, e.LastModifiedByUserId, e.LastModifiedOnDate, e.Status, e.IsOwner, e.RoleId })
                    .HasName("IX_UserRoles_RoleID");

                entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");

                entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserID");

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.LastModifiedByUserId).HasColumnName("LastModifiedByUserID");

                entity.Property(e => e.LastModifiedOnDate).HasColumnType("datetime");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_UserRoles_Roles");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserRoles_Users");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => e.RoleId);

                entity.HasIndex(e => e.BillingFrequency)
                    .HasName("IX_Roles");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.BillingFrequency)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserID");

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.IconFile).HasMaxLength(100);

                entity.Property(e => e.LastModifiedByUserId).HasColumnName("LastModifiedByUserID");

                entity.Property(e => e.LastModifiedOnDate).HasColumnType("datetime");

                entity.Property(e => e.Ordinal).HasDefaultValueSql("((100))");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Rsvpcode)
                    .HasColumnName("RSVPCode")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceFee)
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.TrialFee).HasColumnType("money");

                entity.Property(e => e.TrialFrequency)
                    .HasMaxLength(1)
                    .IsUnicode(false);
            });
        }
    }
}