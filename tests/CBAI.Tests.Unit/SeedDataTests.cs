using CBAI.Web.Data;
using CBAI.Web.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CBAI.Tests.Unit;

/// <summary>
/// Exercises the (not-yet-implemented) demo/seed-data pipeline described in the
/// "Test Foundation &amp; Seed-Data Setup" design note. These tests are expected to fail
/// until <see cref="DemoDataSeeder"/> and <see cref="MemberProfileFaker"/> are implemented —
/// they establish the contract a later slice must satisfy.
/// </summary>
[TestClass]
public sealed class SeedDataTests
{
    [TestMethod]
    public async Task KnownAccounts_AreCreated_WithCorrectRolesAndCanSignIn()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        var options = new SeedDataOptions { Enabled = true };
        await DemoDataSeeder.SeedAsync(services, options);

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = services.GetRequiredService<SignInManager<ApplicationUser>>();

        foreach (var descriptor in KnownAccounts.All)
        {
            var user = await userManager.FindByEmailAsync(descriptor.Email);
            Assert.IsNotNull(user, $"Expected known account '{descriptor.Email}' to exist.");

            var isInRole = await userManager.IsInRoleAsync(user, descriptor.Role);
            Assert.IsTrue(isInRole, $"Expected '{descriptor.Email}' to be in role '{descriptor.Role}'.");

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, descriptor.Password, lockoutOnFailure: false);
            Assert.IsTrue(signInResult.Succeeded, $"Expected '{descriptor.Email}' to sign in with its documented password.");
        }
    }

    [TestMethod]
    public async Task Seeding_IsIdempotent_OnSecondRun()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();

        var options = new SeedDataOptions { Enabled = true };

        await DemoDataSeeder.SeedAsync(services, options);
        var roleCountAfterFirstRun = await db.Roles.CountAsync();
        var userCountAfterFirstRun = await db.Users.CountAsync();

        await DemoDataSeeder.SeedAsync(services, options);
        var roleCountAfterSecondRun = await db.Roles.CountAsync();
        var userCountAfterSecondRun = await db.Users.CountAsync();

        Assert.AreEqual(roleCountAfterFirstRun, roleCountAfterSecondRun, "Re-running the seeder should not create duplicate roles.");
        Assert.AreEqual(userCountAfterFirstRun, userCountAfterSecondRun, "Re-running the seeder should not create duplicate users.");
    }

    [TestMethod]
    public async Task BogusAccounts_AreGenerated_WithConfiguredCount()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var options = new SeedDataOptions { Enabled = true, BogusUserCount = 10 };
        await DemoDataSeeder.SeedAsync(services, options);

        var memberEmails = await userManager.GetUsersInRoleAsync(Roles.Member);
        var knownMemberEmail = KnownAccounts.All.Single(k => k.Role == Roles.Member).Email;
        var bogusEmails = memberEmails.Where(u => u.Email != knownMemberEmail).Select(u => u.Email).ToList();

        Assert.AreEqual(options.BogusUserCount, bogusEmails.Count, "Expected exactly BogusUserCount generated Member accounts.");
        Assert.AreEqual(bogusEmails.Count, bogusEmails.Distinct().Count(), "Expected distinct emails for generated accounts.");
    }

    [TestMethod]
    public void BogusAccounts_AreDeterministic_ForFixedSeed()
    {
        var first = MemberProfileFaker.Generate(count: 5, randomSeed: 20260805);
        var second = MemberProfileFaker.Generate(count: 5, randomSeed: 20260805);

        CollectionAssert.AreEqual(
            first.Select(p => p.Email).ToArray(),
            second.Select(p => p.Email).ToArray(),
            "Generating with the same random seed should produce the same ordered set of profiles.");
    }

    [TestMethod]
    public async Task Seeding_IsSkipped_WhenDisabled()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();

        var options = new SeedDataOptions { Enabled = false };
        await DemoDataSeeder.SeedAsync(services, options);

        Assert.AreEqual(0, await db.Roles.CountAsync(), "No roles should be created when seeding is disabled.");
        Assert.AreEqual(0, await db.Users.CountAsync(), "No users should be created when seeding is disabled.");
    }
}
