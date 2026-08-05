namespace CBAI.Web.Data.Seed;

/// <summary>
/// Options bound from the "SeedData" configuration section. Seeding is opt-in
/// (<see cref="Enabled"/> defaults to <see langword="false"/>) so it never runs in
/// Production unless explicitly turned on by configuration.
/// </summary>
public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool Enabled { get; set; }

    public int BogusUserCount { get; set; } = 40;

    public int RandomSeed { get; set; } = 20260805;
}
