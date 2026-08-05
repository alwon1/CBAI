namespace CBAI.Web.Membership;

/// <summary>
/// Lifecycle states for a <see cref="MembershipApplication"/>. See the "Membership Application
/// Workflow" design note for the full state machine: Draft -&gt; Submitted -&gt; (Approved | Rejected).
/// Both Approved and Rejected are terminal — a decided application cannot be re-decided or
/// resubmitted.
/// </summary>
public enum MembershipApplicationStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
}
