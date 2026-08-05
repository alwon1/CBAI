using CBAI.Web.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CBAI.Web.Data.Configurations;

public sealed class MembershipApplicationConfiguration : IEntityTypeConfiguration<MembershipApplication>
{
    public void Configure(EntityTypeBuilder<MembershipApplication> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ApplicantUserId).IsRequired().HasMaxLength(450);
        builder.Property(a => a.SponsorUserId).HasMaxLength(450);
        builder.Property(a => a.DecidedByUserId).HasMaxLength(450);
        builder.Property(a => a.Status).HasConversion<string>();
        builder.Property(a => a.RequestedMembershipTypeName).IsRequired().HasMaxLength(100);

        builder.HasMany(a => a.AuditEntries)
            .WithOne()
            .HasForeignKey(e => e.MembershipApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
