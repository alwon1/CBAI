using CBAI.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CBAI.Web.Data.Seed;

/// <summary>
/// Orchestrates development/demo seeding: roles, then known accounts, then Bogus-generated
/// accounts. Only runs when <see cref="SeedDataOptions.Enabled"/> is <see langword="true"/>.
/// Each step checks "does this already exist?" before creating, so the seeder is safe to
/// rerun (idempotent) and naturally tops up Bogus accounts if the configured count grows.
/// </summary>
public static class DemoDataSeeder
{
    private const string BogusAccountPassword = "Bogus123!";

    public static async Task SeedAsync(IServiceProvider services, SeedDataOptions options, CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRolesAsync(roleManager, cancellationToken);
        await EnsureKnownAccountsAsync(userManager, cancellationToken);
        await EnsureBogusAccountsAsync(userManager, options, cancellationToken);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, CancellationToken cancellationToken)
    {
        foreach (var role in Roles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                ThrowIfFailed(result, $"Failed to create role '{role}'.");
            }
        }
    }

    private static async Task EnsureKnownAccountsAsync(UserManager<ApplicationUser> userManager, CancellationToken cancellationToken)
    {
        foreach (var descriptor in KnownAccounts.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await EnsureUserAsync(userManager, descriptor.Email, descriptor.Password, descriptor.Role);
        }
    }

    private static async Task EnsureBogusAccountsAsync(UserManager<ApplicationUser> userManager, SeedDataOptions options, CancellationToken cancellationToken)
    {
        if (options.BogusUserCount <= 0)
        {
            return;
        }

        var knownMemberEmail = KnownAccounts.All.Single(a => a.Role == Roles.Member).Email;
        var existingMembers = await userManager.GetUsersInRoleAsync(Roles.Member);
        var existingBogusCount = existingMembers.Count(u => !string.Equals(u.Email, knownMemberEmail, StringComparison.OrdinalIgnoreCase));

        if (existingBogusCount >= options.BogusUserCount)
        {
            // Already topped up (or beyond) the configured count — nothing to do.
            return;
        }

        var profiles = MemberProfileFaker.Generate(options.BogusUserCount, options.RandomSeed);

        foreach (var profile in profiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingUser = await userManager.FindByEmailAsync(profile.Email);
            if (existingUser is not null)
            {
                continue;
            }

            await EnsureUserAsync(userManager, profile.Email, BogusAccountPassword, Roles.Member, profile.PhoneNumber);
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string? phoneNumber = null)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                // RequireConfirmedAccount = true is set in Program.cs, so seeded accounts must
                // be explicitly marked confirmed to be usable for sign-in.
                EmailConfirmed = true,
                PhoneNumber = phoneNumber,
            };

            var createResult = await userManager.CreateAsync(user, password);
            ThrowIfFailed(createResult, $"Failed to create user '{email}'.");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            ThrowIfFailed(roleResult, $"Failed to add user '{email}' to role '{role}'.");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"{message} Errors: {errors}");
        }
    }
}
