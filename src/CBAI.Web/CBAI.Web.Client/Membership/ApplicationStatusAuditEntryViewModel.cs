namespace CBAI.Web.Client.Membership;

/// <summary>A single audit trail row as displayed by <see cref="ApplicationStatusPanel"/>.</summary>
public sealed record ApplicationStatusAuditEntryViewModel(
    ApplicationAuditAction Action,
    DateTimeOffset TimestampUtc,
    string PerformedBy,
    string? Details);
