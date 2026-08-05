using CBAI.Web.Data;
using CBAI.Web.Data.Seed;
using CBAI.Web.Membership;
using Microsoft.AspNetCore.Identity;
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
    public async Task SubmitAsync_WithSponsorInSponsorRole_TransitionsToSubmitted_AndAppendsAudit()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "member@example.com");
        var sponsorId = await UserIdAsync(users, "sponsor@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        var submitted = await service.SubmitAsync(draft.Id, sponsorId);

        Assert.AreEqual(MembershipApplicationStatus.Submitted, submitted.Status);
        Assert.AreEqual(sponsorId, submitted.SponsorUserId);
        Assert.IsNotNull(submitted.SubmittedAtUtc);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);
        Assert.AreEqual(2, auditTrail.Count, "Expected 'Created' then 'Submitted' audit entries.");
        Assert.AreEqual(MembershipApplicationAuditAction.Submitted, auditTrail[1].Action);
        Assert.AreEqual(applicantId, auditTrail[1].PerformedByUserId, "The applicant is the one performing the submit action.");
    }

    [TestMethod]
    public async Task SubmitAsync_WithSponsorInBoardMemberRole_TransitionsToSubmitted()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var (service, users) = await SeedKnownAccountsAsync(scope.ServiceProvider);
        var applicantId = await UserIdAsync(users, "staff@example.com");
        var sponsorId = await UserIdAsync(users, "board@example.com");

        var draft = await service.CreateDraftAsync(applicantId);
        var submitted = await service.SubmitAsync(draft.Id, sponsorId);

        Assert.AreEqual(MembershipApplicationStatus.Submitted, submitted.Status);
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

        var decided = await service.DecideAsync(draft.Id, deciderId, approve: true, notes: "Great fit for the community.");

        Assert.AreEqual(MembershipApplicationStatus.Approved, decided.Status);
        Assert.AreEqual(deciderId, decided.DecidedByUserId);
        Assert.AreEqual("Great fit for the community.", decided.DecisionNotes);
        Assert.IsNotNull(decided.DecidedAtUtc);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);
        Assert.AreEqual(3, auditTrail.Count, "Expected 'Created', 'Submitted', then 'Approved' audit entries.");
        Assert.AreEqual(MembershipApplicationAuditAction.Approved, auditTrail[2].Action);
        Assert.AreEqual(deciderId, auditTrail[2].PerformedByUserId);
        Assert.AreEqual("Great fit for the community.", auditTrail[2].Details);
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
        await service.DecideAsync(draft.Id, deciderId, approve: true);

        await Assert.ThrowsExactlyAsync<InvalidMembershipApplicationTransitionException>(
            () => service.DecideAsync(draft.Id, deciderId, approve: false));
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
        await service.DecideAsync(draft.Id, deciderId, approve: true);

        var auditTrail = await service.GetAuditTrailAsync(draft.Id);

        CollectionAssert.AreEqual(
            new[] { MembershipApplicationAuditAction.Created, MembershipApplicationAuditAction.Submitted, MembershipApplicationAuditAction.Approved },
            auditTrail.Select(e => e.Action).ToArray());

        for (var i = 1; i < auditTrail.Count; i++)
        {
            Assert.IsTrue(auditTrail[i].TimestampUtc >= auditTrail[i - 1].TimestampUtc, "Audit entries should be ordered oldest first.");
        }
    }
}
