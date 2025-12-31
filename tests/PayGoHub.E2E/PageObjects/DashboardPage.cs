using Microsoft.Playwright;

namespace PayGoHub.E2E.PageObjects;

/// <summary>
/// Page Object Model for the Dashboard page.
/// </summary>
public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPage(IPage page)
    {
        _page = page;
        _baseUrl = PlaywrightConfig.BaseUrl;
    }

    // Selectors
    public ILocator TotalRevenueCard => _page.Locator("text=Total Revenue").First.Locator("..").Locator("h3");
    public ILocator CustomersCard => _page.Locator("text=Customers").First.Locator("..").Locator("h3");
    public ILocator ActiveLoansCard => _page.Locator("text=Active Loans").First.Locator("..").Locator("h3");
    public ILocator InstallationsCard => _page.Locator("text=Installations").First.Locator("..").Locator("h3");
    public ILocator RecentPaymentsTable => _page.Locator("text=Recent Payments").First.Locator("..").Locator("..").Locator("table");
    public ILocator PendingInstallationsSection => _page.Locator("text=Pending Installations").First;
    public ILocator SalesbyRegionSection => _page.Locator("text=Sales by Region").First;
    public ILocator SidebarBrand => _page.Locator("text=PayGoHub").First;
    public ILocator AddCustomerButton => _page.Locator("a.btn-primary[href*='Customers/Create']");

    // Navigation links
    public ILocator CustomersLink => _page.Locator("nav >> text=Customers");
    public ILocator PaymentsLink => _page.Locator("nav >> text=Payments");
    public ILocator LoansLink => _page.Locator("nav >> text=Loans");
    public ILocator InstallationsLink => _page.Locator("nav >> text=Installations");
    public ILocator DevicesLink => _page.Locator("nav >> text=Devices");

    // Actions
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/");
    }

    public async Task<bool> IsLoadedAsync()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return await SidebarBrand.IsVisibleAsync();
    }

    public async Task<string> GetTotalRevenueAsync()
    {
        await _page.WaitForSelectorAsync("text=Total Revenue");
        return await TotalRevenueCard.TextContentAsync() ?? "";
    }

    public async Task NavigateToCustomersAsync()
    {
        await CustomersLink.ClickAsync();
        await _page.WaitForURLAsync("**/Customers**");
    }

    public async Task NavigateToPaymentsAsync()
    {
        await PaymentsLink.ClickAsync();
        await _page.WaitForURLAsync("**/Payments**");
    }

    public async Task ClickAddCustomerAsync()
    {
        await AddCustomerButton.ClickAsync();
        await _page.WaitForURLAsync("**/Customers/Create**");
    }
}
