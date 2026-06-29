using Microsoft.Playwright;
using OrangeHRM.Playwright.Framework.Config;

namespace OrangeHRM.Playwright.Framework.Browser;

// ─────────────────────────────────────────────────────────────────────────────
//  BrowserFactory
//  ==============
//  Manages the Playwright object graph for the lifetime of a test run.
//
//  Object hierarchy (one per level, created/owned here or by the caller):
//
//    IPlaywright        — process-level handle; created once via InitAsync()
//      └─ IBrowser      — one per browser type; expensive to create, reused
//           └─ IBrowserContext  — ONE PER TEST (fresh cookies/storage/auth)
//                └─ IPage       — ONE PER TEST (the thing tests interact with)
//
//  Why split Browser from Context from Page?
//  • IBrowser creation downloads nothing and takes ~50 ms — safe to reuse.
//  • IBrowserContext is the isolation boundary (cookies, local storage, auth).
//    Each test MUST get its own context so they don't share session state.
//  • IPage is a tab inside a context; we create one per context/test.
//
//  Usage (called by BaseUiTest.SetUp):
//    await BrowserFactory.InitAsync();                 // once per process
//    var context = await BrowserFactory.CreateContextAsync();
//    var page    = await context.NewPageAsync();
//    // ... test runs ...
//    await context.CloseAsync();                       // in TearDown
//    await BrowserFactory.DisposeAsync();              // once per process
// ─────────────────────────────────────────────────────────────────────────────

public static class BrowserFactory
{
    // ── Process-level singletons ──────────────────────────────────────────────
    // Accessed by multiple test threads; writes happen only during Init/Dispose
    // which NUnit calls on a single thread (OneTimeSetUp / OneTimeTearDown).
    private static IPlaywright? _playwright;
    private static IBrowser?    _browser;

    // ─────────────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <see cref="IPlaywright"/> instance and launches the browser
    /// defined in <see cref="TestSettings.Current"/>.
    /// Call this once from <c>[OneTimeSetUp]</c> in your base test class.
    /// </summary>
    public static async Task InitAsync()
    {
        if (_playwright is not null)
            throw new InvalidOperationException(
                "BrowserFactory.InitAsync() has already been called. " +
                "Call DisposeAsync() before calling Init again.");

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser    = await LaunchBrowserAsync(_playwright, TestSettings.Current);
    }

    /// <summary>
    /// Creates a fresh <see cref="IBrowserContext"/> for one test.
    /// The context has its own cookies, local storage, and auth state —
    /// completely isolated from every other test's context.
    ///
    /// The caller (BaseUiTest.TearDown) is responsible for calling
    /// <c>context.CloseAsync()</c> after the test finishes.
    /// </summary>
    public static async Task<IBrowserContext> CreateContextAsync()
    {
        EnsureInitialized();

        var settings = TestSettings.Current;

        var options = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width  = settings.ViewportWidth,
                Height = settings.ViewportHeight,
            },

            // Ignore HTTPS errors on the demo site (self-signed cert in some environments)
            IgnoreHTTPSErrors = true,

            // Locale and timezone — override if your app is locale-sensitive
            Locale   = "en-US",
            TimezoneId = "America/New_York",
        };

        var context = await _browser!.NewContextAsync(options);

        // Apply default timeouts at the context level so every IPage created
        // from this context inherits them automatically.
        context.SetDefaultTimeout(settings.DefaultTimeoutMs);
        context.SetDefaultNavigationTimeout(settings.NavigationTimeoutMs);

        return context;
    }

    /// <summary>
    /// Closes the browser and releases the Playwright handle.
    /// Call this from <c>[OneTimeTearDown]</c> in your base test class.
    /// </summary>
    public static async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
    }

    /// <summary>
    /// Exposes the raw <see cref="IBrowser"/> for advanced scenarios
    /// (e.g., persistent contexts, CDP sessions).
    /// For normal tests, use <see cref="CreateContextAsync"/> instead.
    /// </summary>
    public static IBrowser Browser
    {
        get
        {
            EnsureInitialized();
            return _browser!;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task<IBrowser> LaunchBrowserAsync(
        IPlaywright playwright,
        TestSettings settings)
    {
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless    = settings.Headless,
            SlowMo      = settings.SlowMotionMs,

            // Args applied to Chromium-based browsers (Chrome / Edge) only.
            // Playwright ignores unknown args for Firefox/WebKit.
            Args = BuildChromiumArgs(settings),
        };

        // Resolve the correct browser engine from the settings string.
        // Playwright's .NET SDK exposes three engines as properties on IPlaywright.
        IBrowserType browserType = settings.Browser.ToLowerInvariant() switch
        {
            "firefox" => playwright.Firefox,
            "webkit"  => playwright.Webkit,
            _         => playwright.Chromium,   // "chromium" | "chrome" | anything else
        };

        return await browserType.LaunchAsync(launchOptions);
    }

    /// <summary>
    /// Returns Chromium launch args that match what the old Selenium DriverFactory
    /// used, plus a few extras that improve stability in containers / CI.
    /// These are ignored by Firefox and WebKit.
    /// </summary>
    private static string[] BuildChromiumArgs(TestSettings settings)
    {
        var args = new List<string>
        {
            "--disable-gpu",
            "--no-sandbox",               // required in Docker / GitHub Actions
            "--disable-dev-shm-usage",    // prevents crashes in low-memory containers
            "--disable-extensions",
            "--disable-infobars",
        };

        // In headed mode on a dev machine, open at the configured viewport size.
        // (In headless mode the viewport is set on the context, not the window.)
        if (!settings.Headless)
        {
            args.Add($"--window-size={settings.ViewportWidth},{settings.ViewportHeight}");
        }

        return args.ToArray();
    }

    private static void EnsureInitialized()
    {
        if (_playwright is null || _browser is null)
            throw new InvalidOperationException(
                "BrowserFactory has not been initialized. " +
                "Call await BrowserFactory.InitAsync() in [OneTimeSetUp] before using the factory.");
    }
}
