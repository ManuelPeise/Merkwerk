using Data.Database.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(UserEntity.DisplayNameMaxLength).IsRequired();
        builder.Property(u => u.PrivacyPolicyVersion).HasMaxLength(UserEntity.PrivacyPolicyVersionMaxLength);
    }
}
