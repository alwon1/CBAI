using CBAI.Web.Client.Membership;
using Microsoft.FluentUI.AspNetCore.Components;

namespace CBAI.Tests.UI;

/// <summary>
/// Verifies <see cref="ApplicationStatusPanel"/> renders the right status, sponsor, decision,
/// and audit trail information for the "Membership Application Workflow" design note. The
/// component only binds a supplied <see cref="ApplicationStatusViewModel"/> (mapping a real
/// server-side application into that view model is a separate, later concern), so these tests
/// exercise real rendering behavior rather than a stub.
/// </summary>
[TestClass]
public sealed class ApplicationStatusPanelTests : Bunit.BunitContext
{
    public ApplicationStatusPanelTests()
    {
        Services.AddFluentUIComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static ApplicationStatusViewModel CreateViewModel(
        ApplicationStatusValue status = ApplicationStatusValue.Draft,
        string? sponsorName = null,
        string? decisionNotes = null,
        IReadOnlyList<ApplicationStatusAuditEntryViewModel>? auditTrail = null)
        => new()
        {
            ApplicantName = "Morgan Member",
            SponsorName = sponsorName,
            Status = status,
            DecisionNotes = decisionNotes,
            AuditTrail = auditTrail ?? [],
        };

    [TestMethod]
    public void DraftWithNoSponsor_ShowsDraftStatus_AndNoSponsorMessage()
    {
        var viewModel = CreateViewModel();

        var cut = Render<ApplicationStatusPanel>(parameters => parameters
            .Add(p => p.Application, viewModel));

        Assert.AreEqual("Draft", cut.Find("[data-testid='status-value']").TextContent);
        Assert.AreEqual("Morgan Member", cut.Find("[data-testid='applicant-name']").TextContent);
        Assert.IsNotNull(cut.Find("[data-testid='no-sponsor']"));
    }

    [TestMethod]
    public void SubmittedWithSponsor_ShowsSponsorName_AndNoDecisionNotes()
    {
        var viewModel = CreateViewModel(status: ApplicationStatusValue.Submitted, sponsorName: "Sasha Sponsor");

        var cut = Render<ApplicationStatusPanel>(parameters => parameters
            .Add(p => p.Application, viewModel));

        Assert.AreEqual("Submitted", cut.Find("[data-testid='status-value']").TextContent);
        Assert.AreEqual("Sasha Sponsor", cut.Find("[data-testid='sponsor-name']").TextContent);
        Assert.IsFalse(cut.Markup.Contains("data-testid=\"decision-notes\""), "A pending (Submitted) application should not show decision notes yet.");
    }

    [TestMethod]
    public void Approved_ShowsDecisionNotes()
    {
        var viewModel = CreateViewModel(
            status: ApplicationStatusValue.Approved,
            sponsorName: "Sasha Sponsor",
            decisionNotes: "Great fit for the community.");

        var cut = Render<ApplicationStatusPanel>(parameters => parameters
            .Add(p => p.Application, viewModel));

        Assert.AreEqual("Approved", cut.Find("[data-testid='status-value']").TextContent);
        Assert.AreEqual("Great fit for the community.", cut.Find("[data-testid='decision-notes']").TextContent);
    }

    [TestMethod]
    public void Rejected_WithoutNotes_ShowsFallbackDecisionText()
    {
        var viewModel = CreateViewModel(status: ApplicationStatusValue.Rejected, sponsorName: "Sasha Sponsor");

        var cut = Render<ApplicationStatusPanel>(parameters => parameters
            .Add(p => p.Application, viewModel));

        Assert.AreEqual("Rejected", cut.Find("[data-testid='status-value']").TextContent);
        Assert.AreEqual("No notes provided.", cut.Find("[data-testid='decision-notes']").TextContent);
    }

    [TestMethod]
    public void NoAuditEntries_ShowsEmptyState()
    {
        var viewModel = CreateViewModel();

        var cut = Render<ApplicationStatusPanel>(parameters => parameters
            .Add(p => p.Application, viewModel));

        Assert.IsNotNull(cut.Find("[data-testid='audit-empty']"));
    }

    [TestMethod]
    public void AuditTrail_RendersOneRowPerEntry_InSuppliedOrder()
    {
        var auditTrail = new[]
        {
            new ApplicationStatusAuditEntryViewModel(ApplicationAuditAction.Created, DateTimeOffset.UtcNow.AddMinutes(-10), "Morgan Member", null),
            new ApplicationStatusAuditEntryViewModel(ApplicationAuditAction.Submitted, DateTimeOffset.UtcNow.AddMinutes(-5), "Morgan Member", null),
            new ApplicationStatusAuditEntryViewModel(ApplicationAuditAction.Approved, DateTimeOffset.UtcNow, "Bailey Board", "Great fit for the community."),
        };
        var viewModel = CreateViewModel(status: ApplicationStatusValue.Approved, sponsorName: "Sasha Sponsor", auditTrail: auditTrail);

        var cut = Render<ApplicationStatusPanel>(parameters => parameters
            .Add(p => p.Application, viewModel));

        var rows = cut.FindAll("[data-testid='audit-entry']");
        Assert.AreEqual(3, rows.Count);
        StringAssert.Contains(rows[0].TextContent, "Created");
        StringAssert.Contains(rows[0].TextContent, "Morgan Member");
        StringAssert.Contains(rows[1].TextContent, "Submitted");
        StringAssert.Contains(rows[2].TextContent, "Approved");
        StringAssert.Contains(rows[2].TextContent, "Bailey Board");
        StringAssert.Contains(rows[2].TextContent, "Great fit for the community.");
    }
}
