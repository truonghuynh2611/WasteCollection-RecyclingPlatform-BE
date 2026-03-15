using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Configurations;

/// <summary>
/// Configuration for Enterprise entity
/// </summary>
public class EnterpriseConfiguration : IEntityTypeConfiguration<Enterprise>
{
    public void Configure(EntityTypeBuilder<Enterprise> builder)
    {
        builder.ToTable("enterprise");

        builder.HasKey(e => e.EnterpriseId)
            .HasName("enterprise_pkey");

        builder.HasIndex(e => e.UserId, "enterprise_userid_key")
            .IsUnique();

        builder.Property(e => e.EnterpriseId)
            .HasColumnName("enterpriseid")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("userid");

        builder.Property(e => e.DistrictId)
            .HasColumnName("districtid");

        builder.Property(e => e.Wastetypes)
            .HasMaxLength(255)
            .HasColumnName("wastetypes");

        builder.Property(e => e.Dailycapacity)
            .HasColumnName("dailycapacity");

        builder.Property(e => e.Currentload)
            .HasDefaultValue(0)
            .HasColumnName("currentload");

        builder.Property(e => e.Status)
            .HasDefaultValue(true)
            .HasColumnName("status");

        // Foreign key relationships
        builder.HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<Enterprise>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_enterprise_user");

        builder.HasOne(d => d.District)
            .WithMany()
            .HasForeignKey(d => d.DistrictId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_enterprise_district");
    }
}
