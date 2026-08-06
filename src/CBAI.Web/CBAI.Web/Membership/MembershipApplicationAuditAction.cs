namespace CBAI.Web.Membership;

/// <summary>An immutable event in an application's workflow history.</summary>
public enum MembershipApplicationAuditAction
{
    Created,
    SponsorshipRequested,
    SponsorshipConfirmed,
    SponsorshipDeclined,
    Submitted,
    Waitlisted,
    Approved,
    Rejected,
    Restored,
}
