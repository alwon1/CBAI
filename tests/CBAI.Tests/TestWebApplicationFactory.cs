using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CBAI.Tests;

internal sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseDirectory;
    private readonly string _connectionString;

    public TestWebApplicationFactory()
    {
        _databaseDirectory = Path.Combine(Path.GetTempPath(), "cbai-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_databaseDirectory);
        _connectionString = $"DataSource={Path.Combine(_databaseDirectory, "app.db")}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(_databaseDirectory))
        {
            Directory.Delete(_databaseDirectory, recursive: true);
        }
    }
}
