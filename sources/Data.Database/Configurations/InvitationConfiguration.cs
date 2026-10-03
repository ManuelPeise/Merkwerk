using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");
        builder.Property(i => i.Email).HasMaxLength(Invitation.EmailMaxLength).IsRequired();
        builder.Property(i => i.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.TokenHash).HasMaxLength(Invitation.TokenHashLength).IsFixedLength().IsRequired();
        builder.Property(i => i.InvitedByName).HasMaxLength(Invitation.InvitedByNameMaxLength).IsRequired();
        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.HasIndex(i => new { i.OrganizationId, i.Email });
        builder.HasOne<Organization>().WithMany().HasForeignKey(i => i.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
