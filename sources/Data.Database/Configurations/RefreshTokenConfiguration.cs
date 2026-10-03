using Data.Database.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.Property(t => t.TokenHash).HasMaxLength(RefreshToken.TokenHashLength).IsFixedLength().IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Two parallel refreshes with the same token: only the first may revoke it (UPDATE … WHERE RevokedAt IS NULL).
        builder.Property(t => t.RevokedAt).IsConcurrencyToken();
        builder.HasIndex(t => t.ChainId);
        builder.HasIndex(t => new { t.UserId, t.RevokedAt });
        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
