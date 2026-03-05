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

    public virtual DbSet<Pointhistory> Pointhistories { get; set; }

    public virtual DbSet<Reportassignment> Reportassignments { get; set; }

    public virtual DbSet<Reportimage> Reportimages { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<Wastereport> Wastereports { get; set; }

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

            entity.ToTable("area");

            entity.Property(e => e.Areaid).HasColumnName("areaid");
            entity.Property(e => e.Districtid).HasColumnName("districtid");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");

            entity.HasOne(d => d.District).WithMany(p => p.Areas)
                .HasForeignKey(d => d.Districtid)
                .HasConstraintName("fk_area_district");
        });

        modelBuilder.Entity<Citizen>(entity =>
        {
            entity.HasKey(e => e.Citizenid).HasName("citizen_pkey");

            entity.ToTable("citizen");

            entity.HasIndex(e => e.Userid, "citizen_userid_key").IsUnique();

            entity.Property(e => e.Citizenid).HasColumnName("citizenid");
            entity.Property(e => e.Totalpoints)
                .HasDefaultValue(0)
                .HasColumnName("totalpoints");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Citizen)
                .HasForeignKey<Citizen>(d => d.Userid)
                .HasConstraintName("fk_citizen_user");
        });

        modelBuilder.Entity<Collector>(entity =>
        {
            entity.HasKey(e => e.Collectorid).HasName("collector_pkey");

            entity.ToTable("collector");

            entity.HasIndex(e => e.Userid, "collector_userid_key").IsUnique();

            entity.Property(e => e.Collectorid).HasColumnName("collectorid");
            entity.Property(e => e.Currenttaskcount)
                .HasDefaultValue(0)
                .HasColumnName("currenttaskcount");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
            entity.Property(e => e.TeamId).HasColumnName("teamid");
            entity.Property(e => e.Userid).HasColumnName("userid");

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

            entity.ToTable("district");

            entity.Property(e => e.Districtid).HasColumnName("districtid");
            entity.Property(e => e.Districtname)
                .HasMaxLength(150)
                .HasColumnName("districtname");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.Property(e => e.Notificationid).HasColumnName("notificationid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Reportid).HasColumnName("reportid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Report).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Reportid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_report");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_notification_user");
        });

        modelBuilder.Entity<Pointhistory>(entity =>
        {
            entity.HasKey(e => e.Pointlogid).HasName("pointhistory_pkey");

            entity.ToTable("pointhistory");

            entity.Property(e => e.Pointlogid).HasColumnName("pointlogid");
            entity.Property(e => e.Citizenid).HasColumnName("citizenid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Pointamount).HasColumnName("pointamount");
            entity.Property(e => e.Reportid).HasColumnName("reportid");
            entity.Property(e => e.Voucherid).HasColumnName("voucherid");

            entity.HasOne(d => d.Citizen).WithMany(p => p.Pointhistories)
                .HasForeignKey(d => d.Citizenid)
                .HasConstraintName("fk_point_citizen");

            entity.HasOne(d => d.Report).WithMany(p => p.Pointhistories)
                .HasForeignKey(d => d.Reportid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_point_report");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Pointhistories)
                .HasForeignKey(d => d.Voucherid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_point_voucher");
        });

        modelBuilder.Entity<Reportassignment>(entity =>
        {
            entity.HasKey(e => e.Assignmentid).HasName("reportassignment_pkey");

            entity.ToTable("reportassignment");

            entity.Property(e => e.Assignmentid).HasColumnName("assignmentid");
            entity.Property(e => e.Reportid).HasColumnName("reportid");
            entity.Property(e => e.TeamId).HasColumnName("teamid");

            entity.HasOne(d => d.Report).WithMany(p => p.Reportassignments)
                .HasForeignKey(d => d.Reportid)
                .HasConstraintName("fk_assignment_report");

            entity.HasOne(d => d.Team).WithMany(p => p.Reportassignments)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("fk_assignment_team");
        });

        modelBuilder.Entity<Reportimage>(entity =>
        {
            entity.HasKey(e => e.Imageid).HasName("reportimage_pkey");

            entity.ToTable("reportimage");

            entity.Property(e => e.Imageid).HasColumnName("imageid");
            entity.Property(e => e.Imageurl).HasColumnName("imageurl");
            entity.Property(e => e.Reportid).HasColumnName("reportid");

            entity.HasOne(d => d.Report).WithMany(p => p.Reportimages)
                .HasForeignKey(d => d.Reportid)
                .HasConstraintName("fk_image_report");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("team_pkey");

            entity.ToTable("team");

            entity.Property(e => e.TeamId).HasColumnName("teamid");
            entity.Property(e => e.Areaid).HasColumnName("areaid");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");

            entity.HasOne(d => d.Area).WithMany(p => p.Teams)
                .HasForeignKey(d => d.Areaid)
                .HasConstraintName("fk_team_area");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(150)
                .HasColumnName("fullname");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Voucherid).HasName("voucher_pkey");

            entity.ToTable("voucher");

            entity.Property(e => e.Voucherid).HasColumnName("voucherid");
            entity.Property(e => e.Pointsrequired).HasColumnName("pointsrequired");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
            entity.Property(e => e.Stockquantity).HasColumnName("stockquantity");
            entity.Property(e => e.Vouchername)
                .HasMaxLength(150)
                .HasColumnName("vouchername");
        });

        modelBuilder.Entity<Wastereport>(entity =>
        {
            entity.HasKey(e => e.Reportid).HasName("wastereport_pkey");

            entity.ToTable("wastereport");

            entity.Property(e => e.Reportid).HasColumnName("reportid");
            entity.Property(e => e.Areaid).HasColumnName("areaid");
            entity.Property(e => e.Citizenid).HasColumnName("citizenid");
            entity.Property(e => e.Citizenlatitude)
                .HasPrecision(10, 8)
                .HasColumnName("citizenlatitude");
            entity.Property(e => e.Citizenlongitude)
                .HasPrecision(11, 8)
                .HasColumnName("citizenlongitude");
            entity.Property(e => e.Collectorlatitude)
                .HasPrecision(10, 8)
                .HasColumnName("collectorlatitude");
            entity.Property(e => e.Collectorlongitude)
                .HasPrecision(11, 8)
                .HasColumnName("collectorlongitude");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Expiretime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiretime");
            entity.Property(e => e.Wastetype)
                .HasMaxLength(100)
                .HasColumnName("wastetype");

            entity.HasOne(d => d.Area).WithMany(p => p.Wastereports)
                .HasForeignKey(d => d.Areaid)
                .HasConstraintName("fk_report_area");

            entity.HasOne(d => d.Citizen).WithMany(p => p.Wastereports)
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
