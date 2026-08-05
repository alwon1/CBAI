namespace CBAI.Web.Data.Seed;

/// <summary>
/// Fixed set of Identity role names used by the app. This is a small, static starter set —
/// dynamic role/permission management is a later slice.
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Staff = "Staff";
    public const string BoardMember = "BoardMember";
    public const string Sponsor = "Sponsor";
    public const string Member = "Member";

    public static readonly IReadOnlyList<string> All =
    [
        Administrator,
        Staff,
        BoardMember,
        Sponsor,
        Member,
    ];
}
