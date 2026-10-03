using Data.Database.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.Property(t => t.TokenHash).HasMaxLength(RefreshTokenEntity.TokenHashLength).IsFixedLength().IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Two parallel refreshes with the same token: only the first may revoke it (UPDATE … WHERE RevokedAt IS NULL).
        builder.Property(t => t.RevokedAt).IsConcurrencyToken();
        builder.HasIndex(t => t.ChainId);
        builder.HasIndex(t => new { t.UserId, t.RevokedAt });
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
