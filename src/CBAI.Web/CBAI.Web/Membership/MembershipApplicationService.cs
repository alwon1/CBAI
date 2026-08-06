using CBAI.Web.Data;
using CBAI.Web.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CBAI.Web.Membership;

public sealed class MembershipApplicationService(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : IMembershipApplicationService
{
    public async Task<MembershipApplication> CreateDraftAsync(string applicantUserId, CancellationToken cancellationToken = default)
    {
        var application = new MembershipApplication
        {
            Id = Guid.NewGuid(),
            ApplicantUserId = applicantUserId,
            RequestedMembershipTypeName = "Standard",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        application.AuditEntries.Add(new MembershipApplicationAuditEntry
        {
            MembershipApplicationId = application.Id,
            Action = MembershipApplicationAuditAction.Created,
            PerformedByUserId = applicantUserId,
            TimestampUtc = application.CreatedAtUtc,
        });

        db.MembershipApplications.Add(application);
        await db.SaveChangesAsync(cancellationToken);
        return application;
    }

    public async Task<MembershipApplication> SubmitAsync(Guid applicationId, string sponsorUserId, CancellationToken cancellationToken = default)
    {
        var application = await db.MembershipApplications
            .SingleOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null)
        {
            throw new InvalidOperationException($"Application '{applicationId}' was not found.");
        }

        if (application.Status != MembershipApplicationStatus.Draft)
        {
            throw new InvalidMembershipApplicationTransitionException($"Application '{applicationId}' cannot be submitted from {application.Status}.");
        }

        if (!await IsSponsorEligibleAsync(sponsorUserId, application.ApplicantUserId, cancellationToken))
        {
            throw new SponsorIneligibleException($"Sponsor '{sponsorUserId}' is not eligible to sponsor '{application.ApplicantUserId}'.");
        }

        application.SponsorUserId = sponsorUserId;
        application.Status = MembershipApplicationStatus.PendingSponsor;
        application.Version++;
        var requestedAtUtc = DateTimeOffset.UtcNow;
        application.AuditEntries.Add(new MembershipApplicationAuditEntry
        {
            MembershipApplicationId = application.Id,
            Action = MembershipApplicationAuditAction.SponsorshipRequested,
            PerformedByUserId = application.ApplicantUserId,
            TimestampUtc = requestedAtUtc,
        });

        await SaveTransitionAsync(application, MembershipApplicationStatus.Draft, cancellationToken);
        return application;
    }

    public async Task<MembershipApplication> ConfirmSponsorshipAsync(Guid applicationId, string sponsorUserId, CancellationToken cancellationToken = default)
    {
        var application = await FindAsync(applicationId, cancellationToken);
        if (application.Status != MembershipApplicationStatus.PendingSponsor || application.SponsorUserId != sponsorUserId)
            throw new InvalidMembershipApplicationTransitionException($"Application '{applicationId}' is not awaiting confirmation from this sponsor.");

        application.Status = MembershipApplicationStatus.Submitted;
        application.Version++;
        application.SubmittedAtUtc = DateTimeOffset.UtcNow;
        AddAudit(application, MembershipApplicationAuditAction.SponsorshipConfirmed, sponsorUserId);
        await SaveTransitionAsync(application, MembershipApplicationStatus.PendingSponsor, cancellationToken);
        return application;
    }

    public async Task<MembershipApplication> DeclineSponsorshipAsync(Guid applicationId, string sponsorUserId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var application = await FindAsync(applicationId, cancellationToken);
        if (application.Status != MembershipApplicationStatus.PendingSponsor || application.SponsorUserId != sponsorUserId)
            throw new InvalidMembershipApplicationTransitionException($"Application '{applicationId}' is not awaiting confirmation from this sponsor.");

        application.Status = MembershipApplicationStatus.Draft;
        application.SponsorUserId = null;
        application.Version++;
        AddAudit(application, MembershipApplicationAuditAction.SponsorshipDeclined, sponsorUserId, notes);
        await SaveTransitionAsync(application, MembershipApplicationStatus.PendingSponsor, cancellationToken);
        return application;
    }



    public async Task<MembershipApplication> DecideAsync(Guid applicationId, string decidedByUserId, bool approve, string? notes = null, CancellationToken cancellationToken = default)
    {
        var application = await db.MembershipApplications
            .SingleOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null)
        {
            throw new InvalidOperationException($"Application '{applicationId}' was not found.");
        }

        if (application.Status is not (MembershipApplicationStatus.Submitted or MembershipApplicationStatus.Waitlisted))
        {
            throw new InvalidMembershipApplicationTransitionException($"Application '{applicationId}' cannot be decided from {application.Status}.");
        }

        var decisionMaker = string.IsNullOrWhiteSpace(decidedByUserId)
            ? null
            : await userManager.FindByIdAsync(decidedByUserId);
        if (decisionMaker is null)
        {
            throw new DecisionMakerUnauthorizedException($"User '{decidedByUserId}' is not authorized to decide membership applications.");
        }

        var decisionMakerRoles = await userManager.GetRolesAsync(decisionMaker);
        var canDecide = decisionMakerRoles.Contains(Roles.Staff, StringComparer.OrdinalIgnoreCase)
            || decisionMakerRoles.Contains(Roles.BoardMember, StringComparer.OrdinalIgnoreCase);
        if (!canDecide)
        {
            throw new DecisionMakerUnauthorizedException($"User '{decidedByUserId}' is not authorized to decide membership applications.");
        }

        application.Status = approve ? MembershipApplicationStatus.Approved : MembershipApplicationStatus.Rejected;
        application.Version++;
        application.DecidedByUserId = decidedByUserId;
        application.DecisionNotes = notes;
        application.DecidedAtUtc = DateTimeOffset.UtcNow;
        application.AuditEntries.Add(new MembershipApplicationAuditEntry
        {
            MembershipApplicationId = application.Id,
            Action = approve ? MembershipApplicationAuditAction.Approved : MembershipApplicationAuditAction.Rejected,
            PerformedByUserId = decidedByUserId,
            TimestampUtc = application.DecidedAtUtc.Value,
            Details = notes,
        });

        await SaveTransitionAsync(application, MembershipApplicationStatus.Submitted, cancellationToken);
        return application;
    }

    public async Task<MembershipApplication> ChangeStatusAsync(Guid applicationId, string performedByUserId, MembershipApplicationStatus status, string? notes = null, CancellationToken cancellationToken = default)
    {
        var application = await FindAsync(applicationId, cancellationToken);
        await EnsureDecisionMakerAsync(performedByUserId);
        var previous = application.Status;
        var allowed = (previous, status) switch
        {
            (MembershipApplicationStatus.Submitted, MembershipApplicationStatus.Waitlisted or MembershipApplicationStatus.Approved or MembershipApplicationStatus.Rejected) => true,
            (MembershipApplicationStatus.Waitlisted, MembershipApplicationStatus.Submitted or MembershipApplicationStatus.Approved or MembershipApplicationStatus.Rejected) => true,
            (MembershipApplicationStatus.Rejected, MembershipApplicationStatus.Submitted) => true,
            _ => false,
        };
        if (!allowed)
            throw new InvalidMembershipApplicationTransitionException($"Application '{applicationId}' cannot move from {previous} to {status}.");

        application.Status = status;
        application.Version++;
        application.DecidedByUserId = performedByUserId;
        application.DecisionNotes = notes;
        application.DecidedAtUtc = status is MembershipApplicationStatus.Approved or MembershipApplicationStatus.Rejected ? DateTimeOffset.UtcNow : null;
        var action = status switch
        {
            MembershipApplicationStatus.Waitlisted => MembershipApplicationAuditAction.Waitlisted,
            MembershipApplicationStatus.Approved => MembershipApplicationAuditAction.Approved,
            MembershipApplicationStatus.Rejected => MembershipApplicationAuditAction.Rejected,
            _ => MembershipApplicationAuditAction.Restored,
        };
        AddAudit(application, action, performedByUserId, notes);
        await SaveTransitionAsync(application, previous, cancellationToken);
        return application;
    }

    public async Task<IReadOnlyList<MembershipApplication>> GetForApplicantAsync(string applicantUserId, CancellationToken cancellationToken = default) =>
        await db.MembershipApplications.Where(a => a.ApplicantUserId == applicantUserId).OrderByDescending(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MembershipApplication>> GetPendingForSponsorAsync(string sponsorUserId, CancellationToken cancellationToken = default) =>
        await db.MembershipApplications.Where(a => a.SponsorUserId == sponsorUserId && a.Status == MembershipApplicationStatus.PendingSponsor).OrderBy(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MembershipApplication>> GetReviewQueueAsync(CancellationToken cancellationToken = default) =>
        await db.MembershipApplications.Where(a => a.Status == MembershipApplicationStatus.Submitted || a.Status == MembershipApplicationStatus.Waitlisted || a.Status == MembershipApplicationStatus.Rejected).OrderBy(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

    private async Task<MembershipApplication> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.MembershipApplications.SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
        ?? throw new InvalidOperationException($"Application '{id}' was not found.");

    private async Task EnsureDecisionMakerAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) throw new DecisionMakerUnauthorizedException($"User '{userId}' is not authorized to decide membership applications.");
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains(Roles.Staff, StringComparer.OrdinalIgnoreCase) && !roles.Contains(Roles.BoardMember, StringComparer.OrdinalIgnoreCase))
            throw new DecisionMakerUnauthorizedException($"User '{userId}' is not authorized to decide membership applications.");
    }

    private static void AddAudit(MembershipApplication application, MembershipApplicationAuditAction action, string userId, string? details = null) =>
        application.AuditEntries.Add(new MembershipApplicationAuditEntry { MembershipApplicationId = application.Id, Action = action, PerformedByUserId = userId, TimestampUtc = DateTimeOffset.UtcNow, Details = details });



    public async Task<bool> IsSponsorEligibleAsync(string sponsorUserId, string applicantUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sponsorUserId) || sponsorUserId == applicantUserId)
        {
            return false;
        }

        var sponsor = await userManager.FindByIdAsync(sponsorUserId);
        if (sponsor is null)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(sponsor);
        var isEligibleRole = roles.Contains(Roles.Sponsor, StringComparer.OrdinalIgnoreCase) || roles.Contains(Roles.BoardMember, StringComparer.OrdinalIgnoreCase);
        return isEligibleRole;
    }

    public async Task<IReadOnlyList<MembershipApplicationAuditEntry>> GetAuditTrailAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        // Ordering by DateTimeOffset isn't translatable to SQL by the SQLite provider, so the
        // (per-application, and therefore small) result set is materialized first and then
        // ordered client-side.
        var entries = await db.MembershipApplicationAuditEntries
            .Where(e => e.MembershipApplicationId == applicationId)
            .ToListAsync(cancellationToken);

        return entries
            .OrderBy(e => e.TimestampUtc)
            .ThenBy(e => e.Id)
            .ToList();
    }

    private async Task SaveTransitionAsync(
        MembershipApplication application,
        MembershipApplicationStatus expectedStatus,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            db.ChangeTracker.Clear();
            throw new InvalidMembershipApplicationTransitionException(
                $"Application '{application.Id}' is no longer {expectedStatus}; another request completed the transition first.",
                ex);
        }
    }
}
