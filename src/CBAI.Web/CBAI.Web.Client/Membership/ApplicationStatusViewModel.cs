namespace CBAI.Web.Client.Membership;

/// <summary>
/// Display model for <see cref="ApplicationStatusPanel"/>. Mapping a server-side
/// <c>MembershipApplication</c> (plus its audit trail) into this view model is the
/// responsibility of the page/component that hosts the panel — kept out of the
/// WebAssembly client project, which must not reference the EF/Identity-backed server project.
/// </summary>
public sealed class ApplicationStatusViewModel
{
    public required string ApplicantName { get; set; }

    public string? SponsorName { get; set; }

    public ApplicationStatusValue Status { get; set; } = ApplicationStatusValue.Draft;

    public string? DecisionNotes { get; set; }

    public IReadOnlyList<ApplicationStatusAuditEntryViewModel> AuditTrail { get; set; } = [];
}
