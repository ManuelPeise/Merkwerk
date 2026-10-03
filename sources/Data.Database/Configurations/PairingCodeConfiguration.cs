using Data.Database.Entities.Devices;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class PairingCodeConfiguration : IEntityTypeConfiguration<PairingCodeEntity>
{
    public void Configure(EntityTypeBuilder<PairingCodeEntity> builder)
    {
        builder.ToTable("PairingCodes");
        builder.Property(c => c.CodeHash).HasMaxLength(PairingCodeEntity.CodeHashLength).IsFixedLength().IsRequired();

        // Not unique: an old, used code may have the same six digits as a new one. Only one usable code per hash exists.
        builder.HasIndex(c => c.CodeHash);

        // Two devices with the same code at once: only the first may mark it used (UPDATE … WHERE UsedAt IS NULL).
        builder.Property(c => c.UsedAt).IsConcurrencyToken();

        // A device and a new code of the family at once: marking the old code used and revoking it exclude each other
        // (UPDATE … WHERE UsedAt IS NULL AND RevokedAt IS NULL), so a replaced code can no longer pair a device.
        builder.Property(c => c.RevokedAt).IsConcurrencyToken();
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
