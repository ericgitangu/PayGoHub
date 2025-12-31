using Microsoft.Playwright;

namespace PayGoHub.E2E.Tests;

/// <summary>
/// E2E tests for application health checks.
/// </summary>
[TestFixture]
[Category("E2E")]
[Category("Smoke")]
public class HealthCheckTests : E2ETestBase
{
    [Test]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        var response = await Page.APIRequest.GetAsync($"{PlaywrightConfig.BaseUrl}/health");

        Assert.That(response.Ok, Is.True, "Health endpoint should return OK status");

        var body = await response.TextAsync();
        Assert.That(body, Does.Contain("healthy"), "Health endpoint should return healthy status");
    }

    [Test]
    public async Task Homepage_ShouldLoad()
    {
        await Page.GotoAsync("/");

        var title = await Page.TitleAsync();

        Assert.That(title, Does.Contain("PayGoHub"), "Page title should contain PayGoHub");
    }

    [Test]
    public async Task Homepage_ShouldNotHaveConsoleErrors()
    {
        var consoleErrors = new List<string>();

        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleErrors.Add(msg.Text);
            }
        };

        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(consoleErrors, Is.Empty, $"Console errors found: {string.Join(", ", consoleErrors)}");
    }

    [Test]
    public async Task StaticAssets_ShouldLoad()
    {
        var failedRequests = new List<string>();

        Page.RequestFailed += (_, request) =>
        {
            failedRequests.Add($"{request.Url} - {request.Failure}");
        };

        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(failedRequests, Is.Empty, $"Failed requests: {string.Join(", ", failedRequests)}");
    }
}
