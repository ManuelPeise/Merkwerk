using Data.Database.Entities.Devices;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<DeviceEntity>
{
    public void Configure(EntityTypeBuilder<DeviceEntity> builder)
    {
        builder.ToTable("Devices");
        builder.Property(d => d.Name).HasMaxLength(DeviceEntity.NameMaxLength).IsRequired();
        builder.Property(d => d.TokenHash).HasMaxLength(DeviceEntity.TokenHashLength).IsFixedLength().IsRequired();
        builder.HasIndex(d => d.TokenHash).IsUnique();
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
