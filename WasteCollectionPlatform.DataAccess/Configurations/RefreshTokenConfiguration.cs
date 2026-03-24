using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Configurations;

/// <summary>
/// Configuration for RefreshToken entity
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refreshtoken");

        builder.HasKey(e => e.RefreshtokenId)
            .HasName("refreshtoken_pkey");

        builder.Property(e => e.RefreshtokenId)
            .HasColumnName("refreshtokenid")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("userid");

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("token");

        builder.Property(e => e.Expiresat)
            .IsRequired()
            .HasColumnName("expiresat");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("createdat");

        builder.Property(e => e.Isrevoked)
            .HasDefaultValue(false)
            .HasColumnName("isrevoked");

        builder.Property(e => e.Revokedat)
            .HasColumnName("revokedat");

        // Foreign key
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refreshtoken_user");

        // Index for faster lookup
        builder.HasIndex(e => e.Token)
            .IsUnique()
            .HasDatabaseName("idx_refreshtoken_token");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_refreshtoken_userid");
    }
}
