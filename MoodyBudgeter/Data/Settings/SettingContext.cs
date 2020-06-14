using Microsoft.EntityFrameworkCore;
using System;

namespace MoodyBudgeter.Data.Settings
{
    public class SettingContext : DbContext
    {
        public virtual DbSet<DbSiteSetting> SiteSettings { get; set; }
        public virtual DbSet<DbSiteList> SiteList { get; set; }
        public virtual DbSet<DbSiteListEntry> SiteListEntry { get; set; }

        public SettingContext() { }

        public SettingContext(DbContextOptions<SettingContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DBConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbSiteSetting>(entity =>
            {
                entity.HasKey(e => e.SiteSettingId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.CultureCode, e.SettingName })
                    .HasName("IX_SiteSettings")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.SiteSettingId).HasColumnName("SiteSettingID");

                entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserID");

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.CultureCode).HasMaxLength(10);

                entity.Property(e => e.LastModifiedByUserId).HasColumnName("LastModifiedByUserID");

                entity.Property(e => e.LastModifiedOnDate).HasColumnType("datetime");

                entity.Property(e => e.SettingName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SettingValue).HasMaxLength(2000);
            });

            modelBuilder.Entity<DbSiteListEntry>(entity =>
            {
                entity.ToTable("SiteListEntry");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateUpdated).HasColumnType("datetime");

                entity.Property(e => e.Value).HasMaxLength(100);

                entity.HasOne(d => d.SiteList)
                    .WithMany(p => p.SiteListEntry)
                    .HasForeignKey(d => d.SiteListId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SiteListEntry_SiteList");
            });

            modelBuilder.Entity<DbSiteList>(entity =>
            {
                entity.ToTable("SiteList");

                entity.Property(e => e.DateCreated)
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Visible)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");
            });
        }
    }
}
