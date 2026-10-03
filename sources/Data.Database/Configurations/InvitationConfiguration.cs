using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<InvitationEntity>
{
    public void Configure(EntityTypeBuilder<InvitationEntity> builder)
    {
        builder.ToTable("Invitations");
        builder.Property(i => i.Email).HasMaxLength(InvitationEntity.EmailMaxLength).IsRequired();
        builder.Property(i => i.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.TokenHash).HasMaxLength(InvitationEntity.TokenHashLength).IsFixedLength().IsRequired();
        builder.Property(i => i.InvitedByName).HasMaxLength(InvitationEntity.InvitedByNameMaxLength).IsRequired();
        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.HasIndex(i => new { i.OrganizationId, i.Email });
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(i => i.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
