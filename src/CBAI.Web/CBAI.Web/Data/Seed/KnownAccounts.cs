namespace CBAI.Web.Data.Seed;

/// <summary>
/// Fixed, known demo accounts — single source of truth shared by the seeder and by tests,
/// so tests never hand-duplicate credentials or role names. These are demo-only
/// credentials, never used in production (seeding is gated by <see cref="SeedDataOptions.Enabled"/>).
/// </summary>
public static class KnownAccounts
{
    public sealed record Descriptor(string Email, string Password, string DisplayName, string Role);

    public static readonly IReadOnlyList<Descriptor> All =
    [
        new("admin@example.com", "Admin123!", "Alex Admin", Roles.Administrator),
        new("staff@example.com", "Staff123!", "Sam Staff", Roles.Staff),
        new("board@example.com", "Board123!", "Bailey Board", Roles.BoardMember),
        new("sponsor@example.com", "Sponsor123!", "Sasha Sponsor", Roles.Sponsor),
        new("member@example.com", "Member123!", "Morgan Member", Roles.Member),
    ];
}
