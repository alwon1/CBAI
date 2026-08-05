namespace CBAI.Web.Membership;

/// <summary>
/// One immutable record of a state transition applied to a <see cref="MembershipApplication"/>.
/// The ordered set of entries for an application is its audit trail.
/// </summary>
public class MembershipApplicationAuditEntry
{
    public Guid Id { get; set; }

    public required Guid MembershipApplicationId { get; set; }

    public MembershipApplicationAuditAction Action { get; set; }

    /// <summary>Identity user id of whoever performed the action (applicant or decider).</summary>
    public required string PerformedByUserId { get; set; }

    public DateTimeOffset TimestampUtc { get; set; }

    /// <summary>Optional free-text context, e.g. decision notes for Approved/Rejected entries.</summary>
    public string? Details { get; set; }
}
