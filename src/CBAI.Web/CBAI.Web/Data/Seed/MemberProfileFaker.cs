using Bogus;

namespace CBAI.Web.Data.Seed;

/// <summary>
/// Generates realistic, deterministic demo member profiles for a given random seed. Using the
/// same <paramref name="count"/> and <paramref name="randomSeed"/> always produces the same
/// ordered set of profiles, so demo databases seeded independently end up identical.
/// </summary>
public static class MemberProfileFaker
{
    public static IReadOnlyList<SeedUserProfile> Generate(int count, int randomSeed)
    {
        // A fresh Faker with a fixed local seed guarantees the same sequence of names/phone
        // numbers every time this method is called with the same randomSeed.
        var faker = new Faker { Random = new Randomizer(randomSeed) };
        var profiles = new List<SeedUserProfile>(count);

        for (var index = 1; index <= count; index++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var fullName = $"{firstName} {lastName}";

            // Emails are derived from name + a deterministic index rather than
            // Faker.Internet.Email() alone, to avoid unlikely collisions between profiles.
            var email = $"{firstName}.{lastName}.{index}@example.com".ToLowerInvariant();
            var phoneNumber = faker.Phone.PhoneNumber();

            profiles.Add(new SeedUserProfile(fullName, email, phoneNumber));
        }

        return profiles;
    }
}
