using CBAI.Web.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CBAI.Web.Data.Configurations;

public sealed class MembershipApplicationAuditEntryConfiguration : IEntityTypeConfiguration<MembershipApplicationAuditEntry>
{
    public void Configure(EntityTypeBuilder<MembershipApplicationAuditEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MembershipApplicationId).IsRequired();
        builder.Property(e => e.PerformedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.Action).HasConversion<string>();
    }
}
