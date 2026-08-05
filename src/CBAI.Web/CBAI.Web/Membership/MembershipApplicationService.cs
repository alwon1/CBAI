using CBAI.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace CBAI.Web.Membership;

/// <summary>
/// EF Core + Identity-backed implementation of <see cref="IMembershipApplicationService"/>.
/// STUB: every member intentionally throws <see cref="NotImplementedException"/>. This class
/// exists only so the "Membership Application Workflow" test suite (see
/// MembershipApplicationServiceTests) compiles ahead of the real implementation — the tests
/// describe the intended draft/submission/decision/audit-trail contract and are expected to
/// fail (red) until this stub is filled in with real persistence, eligibility, and audit logic.
/// </summary>
public sealed class MembershipApplicationService(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : IMembershipApplicationService
{
    public Task<MembershipApplication> CreateDraftAsync(string applicantUserId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<MembershipApplication> SubmitAsync(Guid applicationId, string sponsorUserId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<MembershipApplication> DecideAsync(Guid applicationId, string decidedByUserId, bool approve, string? notes = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> IsSponsorEligibleAsync(string sponsorUserId, string applicantUserId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<MembershipApplicationAuditEntry>> GetAuditTrailAsync(Guid applicationId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
