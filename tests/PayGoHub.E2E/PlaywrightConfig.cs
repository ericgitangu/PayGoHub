using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PayGoHub.E2E;

/// <summary>
/// Base configuration for Playwright E2E tests.
/// Set BASE_URL environment variable to target different environments.
/// </summary>
public class PlaywrightConfig
{
    public static string BaseUrl => Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public static BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
        IgnoreHTTPSErrors = true
    };
}

/// <summary>
/// Base class for all E2E tests with Playwright browser context.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class E2ETestBase : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return PlaywrightConfig.ContextOptions;
    }

    [TearDown]
    public async Task TearDownTest()
    {
        // Take screenshot on failure for debugging
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshotPath = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "TestResults",
                $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            );
            Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
            await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
            TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");
        }
    }

    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        // Cleanup any test artifacts
        TestContext.Out.WriteLine("E2E test suite completed");
    }
}
