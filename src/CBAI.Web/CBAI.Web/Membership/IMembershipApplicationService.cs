namespace CBAI.Web.Membership;

/// <summary>
/// Coordinates the membership application workflow described in the "Membership Application
/// Workflow" design note: draft creation, submission (gated by sponsor eligibility), and
/// staff/board decisions — appending an audit trail entry for every successful transition.
/// </summary>
public interface IMembershipApplicationService
{
    /// <summary>Creates a new Draft application owned by <paramref name="applicantUserId"/>.</summary>
    Task<MembershipApplication> CreateDraftAsync(string applicantUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attaches <paramref name="sponsorUserId"/> and transitions a Draft application to Submitted.
    /// Throws <see cref="SponsorIneligibleException"/> if the sponsor fails eligibility rules, or
    /// <see cref="InvalidMembershipApplicationTransitionException"/> if the application is not
    /// currently a Draft.
    /// </summary>
    Task<MembershipApplication> SubmitAsync(Guid applicationId, string sponsorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a staff/board decision, transitioning a Submitted application to Approved or
    /// Rejected. Throws <see cref="InvalidMembershipApplicationTransitionException"/> if the
    /// application is not currently Submitted (e.g. still a Draft, or already decided).
    /// </summary>
    Task<MembershipApplication> DecideAsync(Guid applicationId, string decidedByUserId, bool approve, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sponsor eligibility rules: the sponsor account must exist, must be in the Sponsor or
    /// BoardMember role, and must not be the applicant themselves.
    /// </summary>
    Task<bool> IsSponsorEligibleAsync(string sponsorUserId, string applicantUserId, CancellationToken cancellationToken = default);

    /// <summary>Returns the application's audit trail, ordered oldest first.</summary>
    Task<IReadOnlyList<MembershipApplicationAuditEntry>> GetAuditTrailAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
