using CBAI.Web.Data;
using CBAI.Web.Data.Seed;
using CBAI.Web.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CBAI.Tests.Unit;

/// <summary>
/// Exercises the membership application workflow described in the
/// "Membership Application Workflow" design note: draft creation, submission gated by sponsor
/// eligibility, staff/board decisions, and the resulting audit trail.
/// </summary>
[TestClass]
public sealed class MembershipApplicationServiceTests
{
    private static async Task<(IMembershipApplicationService Service, UserManager<ApplicationUser> Users)> SeedKnownAccountsAsync(IServiceProvider services)
    {
        // BogusUserCount = 0: these tests only need the fixed known accounts (one per role) to
        // exercise sponsor eligibility; the extra Bogus-generated Member accounts are irrelevant
        // here and would only slow the seeding step down.
        await DemoDataSeeder.SeedAsync(services, new SeedDataOptions { Enabled = true, BogusUserCount = 0 });

        return (
            services.GetRequiredService<IMembershipApplicationService>(),
            services.GetRequiredService<UserManager<ApplicationUser>>());
    }

    private static async Task<string> UserIdAsync(UserManager<ApplicationUser> users, string email)
    {
        var user = await users.FindByEmailAsync(email);
        Assert.IsNotNull(user, $"Expected known account '{email}' to exist.");
        return user.Id;
    }

    [TestMethod]
    public async Task CreateDraftAsync_CreatesDraftApplication_WithCreatedAuditEntry()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");

        var application = await service.CreateDraftAsync(applicantId);

        Assert.AreNotEqual(Guid.Empty, application.Id);
        Assert.AreEqual(applicantId, application.ApplicantUserId);
        Assert.AreEqual(MembershipApplicationStatus.Draft, application.Status);
        Assert.IsNull(application.SponsorUserId);
        Assert.IsNull(application.SubmittedAtUtc);
        Assert.IsNull(application.DecidedAtUtc);

        var auditTrail = await service.GetAuditTrailAsync(application.Id);
        Assert.AreEqual(1, auditTrail.Count, "Expected a single 'Created' audit entry for a fresh draft.");
        Assert.AreEqual(MembershipApplicationAuditAction.Created, auditTrail[0].Action);
        Assert.AreEqual(applicantId, auditTrail[0].PerformedByUserId);
    }

    [TestMethod]
    public async Task SubmitAsync_WithSponsorInSponsorRole_TransitionsToPendingSponsor_AndAppendsAudit()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        var submitted = await service.SubmitAsync(draft.Id, sponsorId);

        Assert.AreEqual(MembershipApplicationStatus.PendingSponsor, submitted.Status);
        Assert.AreEqual(sponsorId, submitted.SponsorUserId);
        Assert.IsNull(submitted.SubmittedAtUtc);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);
        Assert.AreEqual(2, auditTrail.Count, "Expected creation and sponsorship request audit entries.");
        Assert.AreEqual(MembershipApplicationAuditAction.SponsorshipRequested, auditTrail[1].Action);
        Assert.AreEqual(applicantId, auditTrail[1].PerformedByUserId, "The applicant is the one performing the submit action.");
    }

    [TestMethod]
    public async Task SubmitAsync_WithSponsorInBoardMemberRole_TransitionsToPendingSponsor()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "staff@example.com");
        var sponsorId = await UserIdAsync(users, "board@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        var submitted = await service.SubmitAsync(draft.Id, sponsorId);

        Assert.AreEqual(MembershipApplicationStatus.PendingSponsor, submitted.Status);
        Assert.AreEqual(sponsorId, submitted.SponsorUserId);
    }

    [TestMethod]
    public async Task SubmitAsync_WithSponsorInMemberRole_ThrowsSponsorIneligible_AndLeavesApplicationDraft()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "board@example.com");
        var ineligibleSponsorId = await UserIdAsync(users, "member@example.com");

        var draft = await service.CreateDraftAsync(applicantId);

        await Assert.ThrowsExactlyAsync<SponsorIneligibleException>(
            () => service.SubmitAsync(draft.Id, ineligibleSponsorId));

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);
        Assert.AreEqual(1, auditTrail.Count, "A rejected sponsor should not append a 'Submitted' audit entry.");
    }

    [TestMethod]
    public async Task SubmitAsync_WhenApplicantNamesSelfAsSponsor_ThrowsSponsorIneligible()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "sponsor@example.com");

        var draft = await service.CreateDraftAsync(applicantId);

        await Assert.ThrowsExactlyAsync<SponsorIneligibleException>(
            () => service.SubmitAsync(draft.Id, applicantId));
    }

    [TestMethod]
    public async Task SubmitAsync_WhenApplicationAlreadySubmitted_ThrowsInvalidTransition()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);

        await Assert.ThrowsExactlyAsync<InvalidMembershipApplicationTransitionException>(
            () => service.SubmitAsync(draft.Id, sponsorId));
    }

    [TestMethod]
    public async Task DecideAsync_Approve_TransitionsToApproved_AndAppendsDecisionAudit()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");
        var deciderId = await UserIdAsync(users, "board@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);

        var decided = await service.DecideAsync(draft.Id, deciderId, approve: true, notes: "Great fit for the community.");

        Assert.AreEqual(MembershipApplicationStatus.Approved, decided.Status);
        Assert.AreEqual(deciderId, decided.DecidedByUserId);
        Assert.AreEqual("Great fit for the community.", decided.DecisionNotes);
        Assert.IsNotNull(decided.DecidedAtUtc);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);
        Assert.AreEqual(4, auditTrail.Count, "Expected creation, sponsor request, confirmation, and approval audit entries.");
        Assert.AreEqual(MembershipApplicationAuditAction.Approved, auditTrail[3].Action);
        Assert.AreEqual(deciderId, auditTrail[3].PerformedByUserId);
        Assert.AreEqual("Great fit for the community.", auditTrail[3].Details);
    }

    [TestMethod]
    public async Task DecideAsync_Reject_TransitionsToRejected_AndAppendsDecisionAudit()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");
        var deciderId = await UserIdAsync(users, "staff@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);

        var decided = await service.DecideAsync(draft.Id, deciderId, approve: false, notes: "Incomplete references.");

        Assert.AreEqual(MembershipApplicationStatus.Rejected, decided.Status);
        Assert.AreEqual(deciderId, decided.DecidedByUserId);
        Assert.AreEqual("Incomplete references.", decided.DecisionNotes);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);
        Assert.AreEqual(MembershipApplicationAuditAction.Rejected, auditTrail[^1].Action);
    }

    [TestMethod]
    public async Task DecideAsync_WhenApplicationIsStillDraft_ThrowsInvalidTransition()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var deciderId = await UserIdAsync(users, "board@example.com");

        var draft = await service.CreateDraftAsync(applicantId);

        await Assert.ThrowsExactlyAsync<InvalidMembershipApplicationTransitionException>(
            () => service.DecideAsync(draft.Id, deciderId, approve: true));
    }

    [TestMethod]
    public async Task DecideAsync_WhenApplicationAlreadyDecided_ThrowsInvalidTransition()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");
        var deciderId = await UserIdAsync(users, "board@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);
        await service.DecideAsync(draft.Id, deciderId, approve: true);

        await Assert.ThrowsExactlyAsync<InvalidMembershipApplicationTransitionException>(
            () => service.DecideAsync(draft.Id, deciderId, approve: false));
    }

    [TestMethod]
    [DataRow("member@example.com")]
    [DataRow("sponsor@example.com")]
    public async Task DecideAsync_WithUnauthorizedRole_ThrowsAndLeavesApplicationSubmitted(string deciderEmail)
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");
        var deciderId = await UserIdAsync(users, deciderEmail);

        var draft = await service.CreateDraftAsync(applicantId);
        var submitted = await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);

        await Assert.ThrowsExactlyAsync<DecisionMakerUnauthorizedException>(
            () => service.DecideAsync(draft.Id, deciderId, approve: true));

        Assert.AreEqual(MembershipApplicationStatus.Submitted, submitted.Status);
        Assert.IsNull(submitted.DecidedByUserId);
        Assert.IsNull(submitted.DecidedAtUtc);
        Assert.AreEqual(3, (await service.GetAuditTrailAsync(draft.Id)).Count);
    }

    [TestMethod]
    public async Task DecideAsync_WithNonexistentUser_ThrowsAndLeavesApplicationSubmitted()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        var submitted = await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);

        await Assert.ThrowsExactlyAsync<DecisionMakerUnauthorizedException>(
            () => service.DecideAsync(draft.Id, Guid.NewGuid().ToString(), approve: false));

        Assert.AreEqual(MembershipApplicationStatus.Submitted, submitted.Status);
        Assert.IsNull(submitted.DecidedByUserId);
        Assert.IsNull(submitted.DecidedAtUtc);
        Assert.AreEqual(3, (await service.GetAuditTrailAsync(draft.Id)).Count);
    }

    [TestMethod]
    public async Task ConcurrentTransitions_SecondStaleUpdateFailsWithoutAppendingAudit()
    {
        using var factory = new TestWebApplicationFactory();
        using var setupScope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(setupScope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var draft = await service.CreateDraftAsync(applicantId);

        using var firstScope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var secondDb = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var firstCopy = await firstDb.MembershipApplications.SingleAsync(application => application.Id == draft.Id);
        var staleCopy = await secondDb.MembershipApplications.SingleAsync(application => application.Id == draft.Id);

        ApplySubmission(firstCopy, "first-sponsor");
        ApplySubmission(staleCopy, "stale-sponsor");
        await firstDb.SaveChangesAsync();

        await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() => secondDb.SaveChangesAsync());

        secondDb.ChangeTracker.Clear();
        var persisted = await secondDb.MembershipApplications.SingleAsync(application => application.Id == draft.Id);
        var submittedAudits = await secondDb.MembershipApplicationAuditEntries
            .CountAsync(entry => entry.MembershipApplicationId == draft.Id
                && entry.Action == MembershipApplicationAuditAction.Submitted);
        Assert.AreEqual("first-sponsor", persisted.SponsorUserId);
        Assert.AreEqual(1, submittedAudits, "The failed stale transition must not append a second audit entry.");

        static void ApplySubmission(MembershipApplication application, string sponsorId)
        {
            application.Status = MembershipApplicationStatus.Submitted;
            application.SponsorUserId = sponsorId;
            application.SubmittedAtUtc = DateTimeOffset.UtcNow;
            application.Version++;
            application.AuditEntries.Add(new MembershipApplicationAuditEntry
            {
                MembershipApplicationId = application.Id,
                Action = MembershipApplicationAuditAction.Submitted,
                PerformedByUserId = application.ApplicantUserId,
                TimestampUtc = application.SubmittedAtUtc.Value,
            });
        }
    }

    [TestMethod]
    public async Task IsSponsorEligibleAsync_ReturnsTrue_ForSponsorAndBoardMemberRoles()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorRoleId = await UserIdAsync(users, "sponsor@example.com");
        var boardMemberRoleId = await UserIdAsync(users, "board@example.com");

        Assert.IsTrue(await service.IsSponsorEligibleAsync(sponsorRoleId, applicantId));
        Assert.IsTrue(await service.IsSponsorEligibleAsync(boardMemberRoleId, applicantId));
    }

    [TestMethod]
    public async Task IsSponsorEligibleAsync_ReturnsFalse_ForMemberRole()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "board@example.com");
        var memberRoleId = await UserIdAsync(users, "member@example.com");

        Assert.IsFalse(await service.IsSponsorEligibleAsync(memberRoleId, applicantId));
    }

    [TestMethod]
    public async Task IsSponsorEligibleAsync_ReturnsFalse_ForSelfSponsorship()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");

        Assert.IsFalse(await service.IsSponsorEligibleAsync(sponsorId, sponsorId));
    }

    [TestMethod]
    public async Task IsSponsorEligibleAsync_ReturnsFalse_WhenSponsorAccountDoesNotExist()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");

        Assert.IsFalse(await service.IsSponsorEligibleAsync(Guid.NewGuid().ToString(), applicantId));
    }

    [TestMethod]
    public async Task GetAuditTrailAsync_ReturnsEntriesInChronologicalOrder()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");
        var deciderId = await UserIdAsync(users, "board@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        await service.SubmitAsync(draft.Id, sponsorId);
        await service.ConfirmSponsorshipAsync(draft.Id, sponsorId);
        await service.DecideAsync(draft.Id, deciderId, approve: true);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);

        CollectionAssert.AreEqual(
            new[] { MembershipApplicationAuditAction.Created, MembershipApplicationAuditAction.SponsorshipRequested, MembershipApplicationAuditAction.SponsorshipConfirmed, MembershipApplicationAuditAction.Approved },
            auditTrail.Select(e => e.Action).ToArray());

        for (var i = 1; i < auditTrail.Count; i++)
        {
            Assert.IsTrue(auditTrail[i].TimestampUtc >= auditTrail[i - 1].TimestampUtc, "Audit entries should be ordered oldest first.");
        }
    }
}
