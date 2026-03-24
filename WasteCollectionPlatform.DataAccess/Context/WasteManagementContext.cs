using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Context;

public partial class WasteManagementContext : DbContext
{
    public WasteManagementContext(DbContextOptions<WasteManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Citizen> Citizens { get; set; }

    public virtual DbSet<Collector> Collectors { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PointHistory> PointHistories { get; set; }

    public virtual DbSet<ReportAssignment> ReportAssignments { get; set; }

    public virtual DbSet<ReportImage> ReportImages { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<WasteReport> WasteReports { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<PendingRegistration> PendingRegistrations { get; set; }


    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("image_type", new[] { "Citizen", "Collector" })
            .HasPostgresEnum("point_transaction_type", new[] { "Earn", "Redeem" })
            .HasPostgresEnum("report_status", new[] { "Pending", "Assigned", "Processing", "Completed", "Cancelled" })
            .HasPostgresEnum("team_type", new[] { "Main", "Support" })
            .HasPostgresEnum<UserRole>("user_role")
            .HasPostgresEnum<CollectorRole>("collector_role");

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.AreaId).HasName("area_pkey");

            entity.ToTable("Areas");

            entity.Property(e => e.AreaId).HasColumnName("AreaId");
            entity.Property(e => e.DistrictId).HasColumnName("DistrictId");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("Name");

            entity.HasOne(d => d.District).WithMany(p => p.Areas)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("fk_area_district");
        });

        modelBuilder.Entity<Citizen>(entity =>
        {
            entity.HasKey(e => e.CitizenId).HasName("citizen_pkey");

            entity.ToTable("Citizens");

            entity.HasIndex(e => e.UserId, "citizen_userid_key").IsUnique();

            entity.Property(e => e.CitizenId)
                .HasColumnName("CitizenId")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.TotalPoints)
                .HasDefaultValue(0)
                .HasColumnName("TotalPoints");
            entity.Property(e => e.UserId).HasColumnName("UserId");

            entity.HasOne(d => d.User).WithOne(p => p.Citizen)
                .HasForeignKey<Citizen>(d => d.UserId)
                .HasConstraintName("fk_citizen_user");
        });

        modelBuilder.Entity<Collector>(entity =>
        {
            entity.HasKey(e => e.CollectorId).HasName("collector_pkey");

            entity.ToTable("Collectors");

            entity.HasIndex(e => e.UserId, "collector_userid_key").IsUnique();

            entity.Property(e => e.CollectorId).HasColumnName("CollectorId");
            entity.Property(e => e.Role)
                .HasColumnName("Role")
                .HasDefaultValue(CollectorRole.Member);
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("Status");
            entity.Property(e => e.TeamId).HasColumnName("TeamId");
            entity.Property(e => e.UserId).HasColumnName("UserId");

            entity.HasOne(d => d.Team).WithMany(p => p.Collectors)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("fk_collector_team");

            entity.HasOne(d => d.User).WithOne(p => p.Collector)
                .HasForeignKey<Collector>(d => d.UserId)
                .HasConstraintName("fk_collector_user");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("district_pkey");

            entity.ToTable("Districts");

            entity.Property(e => e.DistrictId).HasColumnName("DistrictId");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(150)
                .HasColumnName("DistrictName");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("notification_pkey");

            entity.ToTable("Notifications");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("CreatedAt");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("IsRead");
            entity.Property(e => e.Message).HasColumnName("Message");
            entity.Property(e => e.ReportId).HasColumnName("ReportId");
            entity.Property(e => e.UserId).HasColumnName("UserId");

            entity.HasOne(d => d.Report).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_report");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_notification_user");
        });

        modelBuilder.Entity<PointHistory>(entity =>
        {
            entity.HasKey(e => e.PointlogId).HasName("pointhistory_pkey");

            entity.ToTable("PointHistories");

            entity.Property(e => e.PointlogId).HasColumnName("PointLogId");
            entity.Property(e => e.CitizenId).HasColumnName("CitizenId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("CreatedAt");
            entity.Property(e => e.PointAmount).HasColumnName("PointAmount");
            entity.Property(e => e.ReportId).HasColumnName("ReportId");
            entity.Property(e => e.VoucherId).HasColumnName("VoucherId");

            entity.HasOne(d => d.Citizen).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.CitizenId)
                .HasConstraintName("fk_point_citizen");

            entity.HasOne(d => d.Report).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_point_report");

            entity.HasOne(d => d.Voucher).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_point_voucher");
        });

        modelBuilder.Entity<ReportAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("reportassignment_pkey");

            entity.ToTable("ReportAssignments");

            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentId");
            entity.Property(e => e.ReportId).HasColumnName("ReportId");
            entity.Property(e => e.TeamId).HasColumnName("TeamId");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportAssignments)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("fk_assignment_report");

            entity.HasOne(d => d.Team).WithMany(p => p.ReportAssignments)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("fk_assignment_team");
        });

        modelBuilder.Entity<ReportImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("reportimage_pkey");

            entity.ToTable("ReportImages");

            entity.Property(e => e.ImageId).HasColumnName("ImageId");
            entity.Property(e => e.Imageurl).HasColumnName("ImageUrl");
            entity.Property(e => e.ReportId).HasColumnName("ReportId");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportImages)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("fk_image_report");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("team_pkey");

            entity.ToTable("Teams");

            entity.Property(e => e.TeamId).HasColumnName("TeamId");
            entity.Property(e => e.AreaId).HasColumnName("AreaId");
            entity.Property(e => e.CurrentTaskCount)
                .HasDefaultValue(0)
                .HasColumnName("CurrentTaskCount");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("Name");

            entity.HasOne(d => d.Area).WithMany(p => p.Teams)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("fk_team_area");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.ToTable("Users");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasColumnName("UserId")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("Email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("FullName");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("Password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("Phone");
            entity.Property(e => e.Role)
                .HasColumnName("Role");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("Status");
            entity.Property(e => e.EmailVerified)
                .HasColumnName("EmailVerified");
            entity.Property(e => e.VerificationToken)
                .HasMaxLength(500)
                .HasColumnName("VerificationToken");
            entity.Property(e => e.VerificationTokenExpiry)
                .HasColumnName("VerificationTokenExpiry");
            entity.Property(e => e.ResetPasswordToken)
                .HasMaxLength(500)
                .HasColumnName("ResetPasswordToken");
            entity.Property(e => e.ResetTokenExpiry)
                .HasColumnName("ResetTokenExpiry");
            entity.Property(e => e.TokenVersion)
                .HasColumnName("TokenVersion");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("voucher_pkey");

            entity.ToTable("Vouchers");

            entity.Property(e => e.VoucherId).HasColumnName("VoucherId");
            entity.Property(e => e.PointsRequired).HasColumnName("PointsRequired");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("Status");
            entity.Property(e => e.StockQuantity).HasColumnName("StockQuantity");
            entity.Property(e => e.VoucherName)
                .HasMaxLength(150)
                .HasColumnName("VoucherName");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.VoucherCode).HasMaxLength(50).HasColumnName("VoucherCode");
            entity.Property(e => e.Image).HasColumnName("Image");
            entity.Property(e => e.Category).HasMaxLength(50).HasColumnName("Category");
            entity.Property(e => e.ExpiryDays).HasColumnName("ExpiryDays");
        });

        modelBuilder.Entity<WasteReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("wastereport_pkey");

            entity.ToTable("WasteReports");

            entity.Property(e => e.ReportId).HasColumnName("ReportId");
            entity.Property(e => e.AreaId).HasColumnName("AreaId");
            entity.Property(e => e.CitizenId).HasColumnName("CitizenId");
            entity.Property(e => e.CitizenLatitude)
                .HasPrecision(10, 8)
                .HasColumnName("CitizenLatitude");
            entity.Property(e => e.CitizenLongitude)
                .HasPrecision(11, 8)
                .HasColumnName("CitizenLongitude");
            entity.Property(e => e.CollectorLatitude)
                .HasPrecision(10, 8)
                .HasColumnName("CollectorLatitude");
            entity.Property(e => e.CollectorLongitude)
                .HasPrecision(11, 8)
                .HasColumnName("CollectorLongitude");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("CreatedAt");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.ExpireTime)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("ExpireTime");
            entity.Property(e => e.Status)
                .HasColumnName("Status");
            entity.Property(e => e.TeamId)
                .HasColumnName("TeamId");
            entity.Property(e => e.WasteType)
                .HasMaxLength(100)
                .HasColumnName("WasteType");

            entity.HasOne(d => d.Area).WithMany(p => p.WasteReports)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("fk_report_area");

            entity.HasOne(d => d.Citizen).WithMany(p => p.WasteReports)
                .HasForeignKey(d => d.CitizenId)
                .HasConstraintName("fk_report_citizen");
        });


        modelBuilder.Entity<SystemConfiguration>().HasData(
            new SystemConfiguration { Key = "Points_CompletedReport", Value = "10", Description = "Number of points earned by citizen when a waste report is successfully completed." },
            new SystemConfiguration { Key = "Points_CancelledReport", Value = "-5", Description = "Number of points deducted from citizen when a waste report is invalid/cancelled." }
        );

        // RefreshToken configuration
        modelBuilder.ApplyConfiguration(new Configurations.RefreshTokenConfiguration());


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
