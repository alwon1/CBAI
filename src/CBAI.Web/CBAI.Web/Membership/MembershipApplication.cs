namespace CBAI.Web.Membership;

/// <summary>
/// A prospective member's application: starts as a <see cref="MembershipApplicationStatus.Draft"/>
/// owned by the applicant, moves to <see cref="MembershipApplicationStatus.Submitted"/> once an
/// eligible sponsor is attached, and is finally decided (<see cref="MembershipApplicationStatus.Approved"/>
/// or <see cref="MembershipApplicationStatus.Rejected"/>) by staff/board.
/// </summary>
public class MembershipApplication
{
    public Guid Id { get; set; }

    /// <summary>Identity user id of the prospective member.</summary>
    public required string ApplicantUserId { get; set; }

    public required string RequestedMembershipTypeName { get; set; }

    /// <summary>Identity user id of the sponsor who vouched for the applicant at submission time.</summary>
    public string? SponsorUserId { get; set; }

    public MembershipApplicationStatus Status { get; set; } = MembershipApplicationStatus.Draft;

    /// <summary>Notes recorded by the decider when approving or rejecting the application.</summary>
    public string? DecisionNotes { get; set; }

    /// <summary>Identity user id of the staff/board member who recorded the decision.</summary>
    public string? DecidedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? SubmittedAtUtc { get; set; }

    public DateTimeOffset? DecidedAtUtc { get; set; }

    public List<MembershipApplicationAuditEntry> AuditEntries { get; set; } = [];
}
