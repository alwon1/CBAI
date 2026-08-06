namespace CBAI.Web.Membership;

public interface IMembershipApplicationService
{
    Task<MembershipApplication> CreateDraftAsync(string applicantUserId, CancellationToken cancellationToken = default);
    Task DeleteDraftAsync(Guid applicationId, string applicantUserId, CancellationToken cancellationToken = default);
    Task<MembershipApplication> SubmitAsync(Guid applicationId, string sponsorUserId, CancellationToken cancellationToken = default);
    Task<MembershipApplication> ConfirmSponsorshipAsync(Guid applicationId, string sponsorUserId, CancellationToken cancellationToken = default);
    Task<MembershipApplication> DeclineSponsorshipAsync(Guid applicationId, string sponsorUserId, string? notes = null, CancellationToken cancellationToken = default);
    Task<MembershipApplication> ChangeStatusAsync(Guid applicationId, string performedByUserId, MembershipApplicationStatus status, string? notes = null, CancellationToken cancellationToken = default);
    Task<MembershipApplication> DecideAsync(Guid applicationId, string decidedByUserId, bool approve, string? notes = null, CancellationToken cancellationToken = default);
    Task<bool> IsSponsorEligibleAsync(string sponsorUserId, string applicantUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipApplication>> GetForApplicantAsync(string applicantUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipApplication>> GetPendingForSponsorAsync(string sponsorUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipApplication>> GetReviewQueueAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipApplicationAuditEntry>> GetAuditTrailAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
