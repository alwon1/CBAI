namespace CBAI.Tests.Shared;

/// <summary>
/// Creates a temp-file SQLite database (directory + connection string) for a single test
/// instance and removes it on disposal. Extracted from <see cref="TestWebApplicationFactory"/>
/// so tests that need a raw <c>ApplicationDbContext</c> (not a full <c>WebApplicationFactory</c>)
/// can reuse the same temp-file pattern.
/// </summary>
public sealed class EphemeralSqliteDatabase : IDisposable
{
    private readonly string _databaseDirectory;

    public string ConnectionString { get; }

    public EphemeralSqliteDatabase()
    {
        _databaseDirectory = Path.Combine(Path.GetTempPath(), "cbai-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_databaseDirectory);
        ConnectionString = $"DataSource={Path.Combine(_databaseDirectory, "app.db")}";
    }

    public void Dispose()
    {
        if (Directory.Exists(_databaseDirectory))
        {
            Directory.Delete(_databaseDirectory, recursive: true);
        }
    }
}
