using Data.Database.Entities.Devices;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class PairingCodeConfiguration : IEntityTypeConfiguration<PairingCode>
{
    public void Configure(EntityTypeBuilder<PairingCode> builder)
    {
        builder.ToTable("PairingCodes");
        builder.Property(c => c.CodeHash).HasMaxLength(PairingCode.CodeHashLength).IsFixedLength().IsRequired();

        // Not unique: an old, used code may have the same six digits as a new one. Only one usable code per hash exists.
        builder.HasIndex(c => c.CodeHash);

        // Two devices with the same code at once: only the first may mark it used (UPDATE … WHERE UsedAt IS NULL).
        builder.Property(c => c.UsedAt).IsConcurrencyToken();
        builder.HasOne<Organization>().WithMany().HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
