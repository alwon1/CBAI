using CBAI.Web.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CBAI.Web.Data.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ApplicationUserId).IsRequired().HasMaxLength(450);
        builder.Property(m => m.MembershipTypeName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Status).HasConversion<string>();
    }
}
