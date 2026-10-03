using Data.Database.Entities.Devices;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");
        builder.Property(d => d.Name).HasMaxLength(Device.NameMaxLength).IsRequired();
        builder.Property(d => d.TokenHash).HasMaxLength(Device.TokenHashLength).IsFixedLength().IsRequired();
        builder.HasIndex(d => d.TokenHash).IsUnique();
        builder.HasOne<Organization>().WithMany().HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
