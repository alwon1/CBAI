namespace CBAI.Web.Client.Membership;

/// <summary>
/// Client-safe mirror of <c>CBAI.Web.Membership.MembershipApplicationAuditAction</c>, used to
/// label rows in the audit trail rendered by <see cref="ApplicationStatusPanel"/>.
/// </summary>
public enum ApplicationAuditAction
{
    Created,
    Submitted,
    Approved,
    Rejected,
}
