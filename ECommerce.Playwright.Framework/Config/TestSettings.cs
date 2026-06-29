using Microsoft.Extensions.Configuration;

namespace ECommerce.Playwright.Framework.Config;

// ─────────────────────────────────────────────────────────────────────────────
//  TestSettings
//  ============
//  Single source of truth for all runtime configuration values.
//
//  Loading order (later sources win):
//    1. appsettings.json                    — checked-in defaults
//    2. appsettings.{DOTNET_ENVIRONMENT}.json — per-env overrides (Development / CI)
//    3. Environment variables               — CI secrets / ad-hoc overrides
//
//  Usage in tests:
//    var settings = TestSettings.Current;   // cached singleton
//    var user     = settings.GetUser("Admin");
//    string url   = settings.BaseUrl;
// ─────────────────────────────────────────────────────────────────────────────

public sealed class TestSettings
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    // Loaded once per process; thread-safe via Lazy<T>.
    private static readonly Lazy<TestSettings> _instance =
        new(() => Load(), LazyThreadSafetyMode.PublicationOnly);

    public static TestSettings Current => _instance.Value;

    // ── Core target app ───────────────────────────────────────────────────────
    public string BaseUrl { get; init; } = "https://opensource-demo.orangehrmlive.com/web/index.php";

    // ── User credentials ──────────────────────────────────────────────────────
    // Key = role name (e.g. "Admin", "Employee").  Tests call GetUser("Admin").
    public Dictionary<string, UserCredentials> Users { get; init; } = new();

    // ── Browser ───────────────────────────────────────────────────────────────
    // Valid values: Chromium | Firefox | Webkit
    public string Browser { get; init; } = "Chromium";
    public bool Headless { get; init; } = false;
    public int SlowMotionMs { get; init; } = 0;
    public int ViewportWidth { get; init; } = 1920;
    public int ViewportHeight { get; init; } = 1080;

    // ── Timeouts ──────────────────────────────────────────────────────────────
    public int DefaultTimeoutMs { get; init; } = 30_000;
    public int NavigationTimeoutMs { get; init; } = 60_000;

    // ── Retry ─────────────────────────────────────────────────────────────────
    // Number of times a test is retried on failure.
    // BaseUiTest reads this and passes it to the NUnit RetryAttribute at runtime.
    public int RetryCount { get; init; } = 1;

    // ── Parallelism ───────────────────────────────────────────────────────────
    // This value is read by the test projects' AssemblyInfo.cs to set
    // [assembly: LevelOfParallelism(n)].  Start at 1; increase incrementally.
    public int ParallelWorkers { get; init; } = 1;

    // ── Tracing / artifact capture ────────────────────────────────────────────
    public TraceMode TraceMode { get; init; } = TraceMode.OnFailure;
    public bool RecordVideo { get; init; } = false;
    public string ArtifactsDirectory { get; init; } = "TestResults";

    // ── API testing ───────────────────────────────────────────────────────────
    public string ApiBaseUrl { get; init; } = "https://opensource-demo.orangehrmlive.com/web/index.php/api/v2";
    public string RestfulBookerBaseUrl { get; init; } = "https://restful-booker.herokuapp.com";

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns credentials for the requested role.
    /// Throws <see cref="KeyNotFoundException"/> with a helpful message if the
    /// role is not in the config — catches typos early rather than getting a
    /// null-ref inside a test.
    /// </summary>
    public UserCredentials GetUser(string role)
    {
        if (Users.TryGetValue(role, out var creds))
            return creds;

        var available = string.Join(", ", Users.Keys);
        throw new KeyNotFoundException(
            $"No user credentials found for role '{role}'. " +
            $"Available roles in appsettings.json → Users: [{available}]");
    }

    /// <summary>
    /// Convenience shortcut — most tests need the admin user.
    /// </summary>
    public UserCredentials AdminUser => GetUser("Admin");

    // ─────────────────────────────────────────────────────────────────────────
    //  Factory / loader
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds configuration using the layered provider chain described in the
    /// file header comment, then binds it to a new <see cref="TestSettings"/>.
    /// </summary>
    internal static TestSettings Load()
    {
        // Resolve which environment we're in.
        // GitHub Actions workflow sets DOTNET_ENVIRONMENT=CI.
        // Local dev: unset → defaults to "Development".
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                       ?? "Development";

        // Find the Config/ folder next to this assembly.
        // Works whether running from bin/Debug or from dotnet test --output.
        var configDir = Path.Combine(
            AppContext.BaseDirectory, "Config");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(configDir) ? configDir : AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();   // highest priority — CI secrets / ad-hoc overrides

        var config = builder.Build();

        var settings = new TestSettings
        {
            BaseUrl             = config["BaseUrl"]?.TrimEnd('/') ?? "https://opensource-demo.orangehrmlive.com/web/index.php",
            Browser             = config["Browser"] ?? "Chromium",
            Headless            = ParseBool(config["Headless"], defaultValue: false),
            SlowMotionMs        = ParseInt(config["SlowMotionMs"], defaultValue: 0),
            ViewportWidth       = ParseInt(config["ViewportWidth"], defaultValue: 1920),
            ViewportHeight      = ParseInt(config["ViewportHeight"], defaultValue: 1080),
            DefaultTimeoutMs    = ParseInt(config["DefaultTimeoutMs"], defaultValue: 30_000),
            NavigationTimeoutMs = ParseInt(config["NavigationTimeoutMs"], defaultValue: 60_000),
            RetryCount          = ParseInt(config["RetryCount"], defaultValue: 1),
            ParallelWorkers     = ParseInt(config["ParallelWorkers"], defaultValue: 1),
            TraceMode           = ParseEnum<TraceMode>(config["TraceMode"], defaultValue: TraceMode.OnFailure),
            RecordVideo         = ParseBool(config["RecordVideo"], defaultValue: true),
            ArtifactsDirectory  = config["ArtifactsDirectory"] ?? "TestResults",
            ApiBaseUrl          = config["ApiBaseUrl"]?.TrimEnd('/') ?? string.Empty,
            RestfulBookerBaseUrl = config["RestfulBookerBaseUrl"]?.TrimEnd('/') ?? "https://restful-booker.herokuapp.com",
            Users               = BindUsers(config),
        };

        return settings;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Private parse helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, UserCredentials> BindUsers(IConfiguration config)
    {
        var result = new Dictionary<string, UserCredentials>(StringComparer.OrdinalIgnoreCase);
        var section = config.GetSection("Users");

        foreach (var child in section.GetChildren())
        {
            result[child.Key] = new UserCredentials(
                Username: child["Username"] ?? string.Empty,
                Password: child["Password"] ?? string.Empty);
        }

        return result;
    }

    private static bool ParseBool(string? value, bool defaultValue)
        => bool.TryParse(value, out var result) ? result : defaultValue;

    private static int ParseInt(string? value, int defaultValue)
        => int.TryParse(value, out var result) && result > 0 ? result : defaultValue;

    private static T ParseEnum<T>(string? value, T defaultValue) where T : struct, Enum
        => Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : defaultValue;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Supporting types (kept in the same file to reduce scatter while Config is small)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Immutable credentials record.  'record' gives us structural equality for
/// free — useful if tests ever compare user objects.
/// </summary>
public sealed record UserCredentials(string Username, string Password);

/// <summary>
/// Controls when Playwright trace zips are written to ArtifactsDirectory.
/// </summary>
public enum TraceMode
{
    /// <summary>Always write a trace zip — useful for local debugging sessions.</summary>
    Always,

    /// <summary>Only write a trace zip when the test fails (default).</summary>
    OnFailure,

    /// <summary>Never write traces — use for performance-sensitive smoke runs.</summary>
    Never
}
