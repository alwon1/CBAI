namespace CBAI.Web.Membership;

/// <summary>Lifecycle states for a membership application.</summary>
public enum MembershipApplicationStatus
{
    Draft,
    PendingSponsor,
    Submitted,
    Waitlisted,
    Approved,
    Rejected,
}
