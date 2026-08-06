namespace CBAI.Web.Client.Membership;

/// <summary>
/// Client-safe mirror of <c>CBAI.Web.Membership.MembershipApplicationStatus</c>. Kept as an
/// independent type (rather than a shared reference) because <c>CBAI.Web.Client</c> is a
/// WebAssembly project that must not depend on the server-only EF/Identity-backed project.
/// </summary>
public enum ApplicationStatusValue
{
    Draft,
    PendingSponsor,
    Submitted,
    Waitlisted,
    Approved,
    Rejected,
}
