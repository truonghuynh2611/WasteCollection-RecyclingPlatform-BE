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
        builder.ToTable("RefreshTokens");

        builder.HasKey(e => e.RefreshTokenId)
            .HasName("refreshtoken_pkey");

        builder.Property(e => e.RefreshTokenId)
            .HasColumnName("RefreshTokenId")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("UserId");

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("Token");

        builder.Property(e => e.ExpiresAt)
            .IsRequired()
            .HasColumnName("ExpiresAt");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("CreatedAt");

        builder.Property(e => e.IsRevoked)
            .HasDefaultValue(false)
            .HasColumnName("IsRevoked");

        builder.Property(e => e.RevokedAt)
            .HasColumnName("RevokedAt");

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
