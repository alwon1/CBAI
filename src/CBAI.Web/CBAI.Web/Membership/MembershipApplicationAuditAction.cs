namespace CBAI.Web.Membership;

/// <summary>
/// The kind of event recorded in a <see cref="MembershipApplication"/>'s audit trail. One entry
/// is appended for every successful state transition, so the full history of an application can
/// be reconstructed later (who did what, and when).
/// </summary>
public enum MembershipApplicationAuditAction
{
    Created,
    Submitted,
    Approved,
    Rejected,
}
