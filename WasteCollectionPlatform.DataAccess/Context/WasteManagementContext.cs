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

    public virtual DbSet<Enterprise> Enterprises { get; set; }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("image_type", new[] { "Citizen", "Collector" })
            .HasPostgresEnum("point_transaction_type", new[] { "Earn", "Redeem" })
            .HasPostgresEnum("report_status", new[] { "Pending", "Accepted", "Assigned", "OnTheWay", "Collected", "Failed" })
            .HasPostgresEnum("team_type", new[] { "Main", "Support" })
            .HasPostgresEnum<UserRole>("user_role");

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Areaid).HasName("area_pkey");

            entity.ToTable("Areas");

            entity.Property(e => e.Areaid).HasColumnName("AreaId");
            entity.Property(e => e.Districtid).HasColumnName("DistrictId");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("Name");

            entity.HasOne(d => d.District).WithMany(p => p.Areas)
                .HasForeignKey(d => d.Districtid)
                .HasConstraintName("fk_area_district");
        });

        modelBuilder.Entity<Citizen>(entity =>
        {
            entity.HasKey(e => e.Citizenid).HasName("citizen_pkey");

            entity.ToTable("Citizens");

            entity.HasIndex(e => e.Userid, "citizen_userid_key").IsUnique();

            entity.Property(e => e.Citizenid).HasColumnName("CitizenId");
            entity.Property(e => e.Totalpoints)
                .HasDefaultValue(0)
                .HasColumnName("TotalPoints");
            entity.Property(e => e.Userid).HasColumnName("UserId");

            entity.HasOne(d => d.User).WithOne(p => p.Citizen)
                .HasForeignKey<Citizen>(d => d.Userid)
                .HasConstraintName("fk_citizen_user");
        });

        modelBuilder.Entity<Collector>(entity =>
        {
            entity.HasKey(e => e.Collectorid).HasName("collector_pkey");

            entity.ToTable("Collectors");

            entity.HasIndex(e => e.Userid, "collector_userid_key").IsUnique();

            entity.Property(e => e.Collectorid).HasColumnName("CollectorId");
            entity.Property(e => e.Currenttaskcount)
                .HasDefaultValue(0)
                .HasColumnName("CurrentTaskCount");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("Status");
            entity.Property(e => e.TeamId).HasColumnName("TeamId");
            entity.Property(e => e.Userid).HasColumnName("UserId");

            entity.HasOne(d => d.Team).WithMany(p => p.Collectors)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("fk_collector_team");

            entity.HasOne(d => d.User).WithOne(p => p.Collector)
                .HasForeignKey<Collector>(d => d.Userid)
                .HasConstraintName("fk_collector_user");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Districtid).HasName("district_pkey");

            entity.ToTable("Districts");

            entity.Property(e => e.Districtid).HasColumnName("DistrictId");
            entity.Property(e => e.Districtname)
                .HasMaxLength(150)
                .HasColumnName("DistrictName");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("notification_pkey");

            entity.ToTable("Notifications");

            entity.Property(e => e.Notificationid).HasColumnName("NotificationId");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("CreatedAt");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("IsRead");
            entity.Property(e => e.Message).HasColumnName("Message");
            entity.Property(e => e.Reportid).HasColumnName("ReportId");
            entity.Property(e => e.Userid).HasColumnName("UserId");

            entity.HasOne(d => d.Report).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Reportid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_report");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_notification_user");
        });

        modelBuilder.Entity<PointHistory>(entity =>
        {
            entity.HasKey(e => e.Pointlogid).HasName("pointhistory_pkey");

            entity.ToTable("PointHistories");

            entity.Property(e => e.Pointlogid).HasColumnName("PointLogId");
            entity.Property(e => e.Citizenid).HasColumnName("CitizenId");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("CreatedAt");
            entity.Property(e => e.Pointamount).HasColumnName("PointAmount");
            entity.Property(e => e.Reportid).HasColumnName("ReportId");
            entity.Property(e => e.Voucherid).HasColumnName("VoucherId");

            entity.HasOne(d => d.Citizen).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.Citizenid)
                .HasConstraintName("fk_point_citizen");

            entity.HasOne(d => d.Report).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.Reportid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_point_report");

            entity.HasOne(d => d.Voucher).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.Voucherid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_point_voucher");
        });

        modelBuilder.Entity<ReportAssignment>(entity =>
        {
            entity.HasKey(e => e.Assignmentid).HasName("reportassignment_pkey");

            entity.ToTable("ReportAssignments");

            entity.Property(e => e.Assignmentid).HasColumnName("AssignmentId");
            entity.Property(e => e.Reportid).HasColumnName("ReportId");
            entity.Property(e => e.TeamId).HasColumnName("TeamId");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportAssignments)
                .HasForeignKey(d => d.Reportid)
                .HasConstraintName("fk_assignment_report");

            entity.HasOne(d => d.Team).WithMany(p => p.ReportAssignments)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("fk_assignment_team");
        });

        modelBuilder.Entity<ReportImage>(entity =>
        {
            entity.HasKey(e => e.Imageid).HasName("reportimage_pkey");

            entity.ToTable("ReportImages");

            entity.Property(e => e.Imageid).HasColumnName("ImageId");
            entity.Property(e => e.Imageurl).HasColumnName("ImageUrl");
            entity.Property(e => e.Reportid).HasColumnName("ReportId");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportImages)
                .HasForeignKey(d => d.Reportid)
                .HasConstraintName("fk_image_report");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("team_pkey");

            entity.ToTable("Teams");

            entity.Property(e => e.TeamId).HasColumnName("TeamId");
            entity.Property(e => e.Areaid).HasColumnName("AreaId");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("Name");

            entity.HasOne(d => d.Area).WithMany(p => p.Teams)
                .HasForeignKey(d => d.Areaid)
                .HasConstraintName("fk_team_area");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("User_pkey");

            entity.ToTable("Users");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("UserId");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("Email");
            entity.Property(e => e.Fullname)
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
            entity.Property(e => e.Emailverified)
                .HasDefaultValue(false)
                .HasColumnName("EmailVerified");
            entity.Property(e => e.Verificationtoken)
                .HasMaxLength(500)
                .HasColumnName("VerificationToken");
            entity.Property(e => e.Verificationtokenexpiry)
                .HasColumnName("VerificationTokenExpiry");
            entity.Property(e => e.Resetpasswordtoken)
                .HasMaxLength(500)
                .HasColumnName("ResetPasswordToken");
            entity.Property(e => e.Resettokenexpiry)
                .HasColumnName("ResetTokenExpiry");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Voucherid).HasName("voucher_pkey");

            entity.ToTable("Vouchers");

            entity.Property(e => e.Voucherid).HasColumnName("VoucherId");
            entity.Property(e => e.Pointsrequired).HasColumnName("PointsRequired");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("Status");
            entity.Property(e => e.Stockquantity).HasColumnName("StockQuantity");
            entity.Property(e => e.Vouchername)
                .HasMaxLength(150)
                .HasColumnName("VoucherName");
        });

        modelBuilder.Entity<WasteReport>(entity =>
        {
            entity.HasKey(e => e.Reportid).HasName("wastereport_pkey");

            entity.ToTable("WasteReports");

            entity.Property(e => e.Reportid).HasColumnName("ReportId");
            entity.Property(e => e.Areaid).HasColumnName("AreaId");
            entity.Property(e => e.Citizenid).HasColumnName("CitizenId");
            entity.Property(e => e.Citizenlatitude)
                .HasPrecision(10, 8)
                .HasColumnName("CitizenLatitude");
            entity.Property(e => e.Citizenlongitude)
                .HasPrecision(11, 8)
                .HasColumnName("CitizenLongitude");
            entity.Property(e => e.Collectorlatitude)
                .HasPrecision(10, 8)
                .HasColumnName("CollectorLatitude");
            entity.Property(e => e.Collectorlongitude)
                .HasPrecision(11, 8)
                .HasColumnName("CollectorLongitude");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("CreatedAt");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.Expiretime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ExpireTime");
            entity.Property(e => e.Wastetype)
                .HasMaxLength(100)
                .HasColumnName("WasteType");

            entity.HasOne(d => d.Area).WithMany(p => p.WasteReports)
                .HasForeignKey(d => d.Areaid)
                .HasConstraintName("fk_report_area");

            entity.HasOne(d => d.Citizen).WithMany(p => p.WasteReports)
                .HasForeignKey(d => d.Citizenid)
                .HasConstraintName("fk_report_citizen");
        });

        // RefreshToken configuration
        modelBuilder.ApplyConfiguration(new Configurations.RefreshTokenConfiguration());

        // Enterprise configuration
        modelBuilder.ApplyConfiguration(new Configurations.EnterpriseConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
