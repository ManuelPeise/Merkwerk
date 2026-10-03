using Data.Database.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(User.DisplayNameMaxLength).IsRequired();
        builder.Property(u => u.PrivacyPolicyVersion).HasMaxLength(User.PrivacyPolicyVersionMaxLength);
    }
}
