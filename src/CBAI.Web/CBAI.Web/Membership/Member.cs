namespace CBAI.Web.Membership;

public class Member
{
    public Guid Id { get; set; }

    public required string ApplicationUserId { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;

    public required string MembershipTypeName { get; set; }

    public DateOnly? ShareholderSince { get; set; }
}
