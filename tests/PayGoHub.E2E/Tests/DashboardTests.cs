using PayGoHub.E2E.PageObjects;

namespace PayGoHub.E2E.Tests;

/// <summary>
/// E2E tests for the Dashboard page.
/// Run with: dotnet test tests/PayGoHub.E2E --filter "FullyQualifiedName~DashboardTests"
/// </summary>
[TestFixture]
[Category("E2E")]
public class DashboardTests : E2ETestBase
{
    private DashboardPage _dashboard = null!;

    [SetUp]
    public void Setup()
    {
        _dashboard = new DashboardPage(Page);
    }

    [Test]
    public async Task Dashboard_ShouldDisplayBrand()
    {
        await _dashboard.NavigateAsync();

        var isLoaded = await _dashboard.IsLoadedAsync();

        Assert.That(isLoaded, Is.True, "Dashboard should display PayGoHub brand");
    }

    [Test]
    public async Task Dashboard_ShouldDisplayKPICards()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        // Check all KPI cards are visible
        await Expect(_dashboard.TotalRevenueCard).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_ShouldDisplayRevenueAmount()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        var revenue = await _dashboard.GetTotalRevenueAsync();

        Assert.That(revenue, Does.Contain("KES"), "Revenue should display in KES currency");
    }

    [Test]
    public async Task Dashboard_ShouldDisplayRecentPayments()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        await Expect(_dashboard.RecentPaymentsTable).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_ShouldDisplaySalesByRegion()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        await Expect(_dashboard.SalesbyRegionSection).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_ShouldNavigateToCustomers()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        await _dashboard.NavigateToCustomersAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers.*"));
    }

    [Test]
    public async Task Dashboard_AddCustomerButton_ShouldNavigateToCreateForm()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        await _dashboard.ClickAddCustomerAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers/Create.*"));
    }

    [Test]
    public async Task Dashboard_SidebarNavigation_ShouldWork()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.IsLoadedAsync();

        // Test each navigation link
        await _dashboard.CustomersLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers.*"));

        await _dashboard.NavigateAsync();
        await _dashboard.PaymentsLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Payments.*"));

        await _dashboard.NavigateAsync();
        await _dashboard.LoansLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Loans.*"));
    }
}
