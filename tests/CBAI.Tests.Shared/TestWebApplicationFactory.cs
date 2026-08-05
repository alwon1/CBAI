using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CBAI.Tests.Shared;

/// <summary>
/// Spins up the CBAI web app against a temp-file SQLite database that is unique per test
/// instance and cleaned up on dispose. Shared by unit and UI (integration-style) tests.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly EphemeralSqliteDatabase _database = new();

    public string ConnectionString => _database.ConnectionString;

    public TestWebApplicationFactory()
    {
        // Program.cs reads ConnectionStrings:DefaultConnection eagerly (before builder.Build()),
        // so an override applied only via ConfigureAppConfiguration below runs too late to be
        // observed by that early read. Setting the process environment variable ensures the
        // override is already present in configuration by the time WebApplication.CreateBuilder
        // runs inside the deferred host.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _database.ConnectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database.ConnectionString,

                // Disable the app's own startup seeding here: tests drive seeding explicitly
                // (via DemoDataSeeder.SeedAsync with test-specific options) against the fresh
                // per-instance database, and must not have that state pre-populated by the
                // Development-default "SeedData:Enabled" config the ambient app host would
                // otherwise pick up.
                ["SeedData:Enabled"] = "false",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _database.Dispose();
        }
    }
}
