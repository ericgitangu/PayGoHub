using Microsoft.Playwright;

namespace PayGoHub.E2E.PageObjects;

/// <summary>
/// Page Object Model for the Customers pages (list, create, edit, details).
/// </summary>
public class CustomersPage
{
    private readonly IPage _page;

    public CustomersPage(IPage page)
    {
        _page = page;
    }

    // Index page selectors
    public ILocator PageTitle => _page.Locator("h4:has-text('Customers')");
    public ILocator CustomerTable => _page.Locator("table.table");
    public ILocator CustomerRows => _page.Locator("table.table tbody tr");
    public ILocator CreateButton => _page.Locator("a:has-text('Add Customer')");
    public ILocator SearchInput => _page.Locator("input[placeholder*='Search']");

    // Form selectors
    public ILocator FirstNameInput => _page.Locator("#FirstName");
    public ILocator LastNameInput => _page.Locator("#LastName");
    public ILocator EmailInput => _page.Locator("#Email");
    public ILocator PhoneNumberInput => _page.Locator("#PhoneNumber");
    public ILocator RegionInput => _page.Locator("#Region");
    public ILocator DistrictInput => _page.Locator("#District");
    public ILocator AddressInput => _page.Locator("#Address");
    public ILocator SubmitButton => _page.Locator("button[type='submit']");
    public ILocator CancelButton => _page.Locator("a:has-text('Cancel')");

    // Actions - Navigation
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/Customers");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task NavigateToCreateAsync()
    {
        await _page.GotoAsync("/Customers/Create");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickCreateButtonAsync()
    {
        await CreateButton.ClickAsync();
        await _page.WaitForURLAsync("**/Customers/Create**");
    }

    // Actions - CRUD
    public async Task<int> GetCustomerCountAsync()
    {
        await _page.WaitForSelectorAsync("table.table tbody tr");
        return await CustomerRows.CountAsync();
    }

    public async Task FillCustomerFormAsync(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string region,
        string district,
        string address)
    {
        await FirstNameInput.FillAsync(firstName);
        await LastNameInput.FillAsync(lastName);
        await EmailInput.FillAsync(email);
        await PhoneNumberInput.FillAsync(phoneNumber);
        // Region is a select element, District is an input
        await RegionInput.SelectOptionAsync(new SelectOptionValue { Label = region });
        await DistrictInput.FillAsync(district);
        await AddressInput.FillAsync(address);
    }

    public async Task SubmitFormAsync()
    {
        await SubmitButton.ClickAsync();
        await _page.WaitForURLAsync("**/Customers**", new() { Timeout = 10000 });
    }

    public async Task<bool> CustomerExistsAsync(string name)
    {
        var row = _page.Locator($"table.table tbody tr:has-text('{name}')");
        return await row.CountAsync() > 0;
    }

    public async Task ClickEditForCustomerAsync(string name)
    {
        var row = _page.Locator($"table.table tbody tr:has-text('{name}')");
        var editButton = row.Locator("a:has-text('Edit')");
        await editButton.ClickAsync();
        await _page.WaitForURLAsync("**/Customers/Edit/**");
    }

    public async Task ClickDeleteForCustomerAsync(string name)
    {
        var row = _page.Locator($"table.table tbody tr:has-text('{name}')");
        var deleteButton = row.Locator("button:has-text('Delete')");
        await deleteButton.ClickAsync();
    }

    public async Task ClickDetailsForCustomerAsync(string name)
    {
        var row = _page.Locator($"table.table tbody tr:has-text('{name}')");
        var detailsButton = row.Locator("a:has-text('Details')");
        await detailsButton.ClickAsync();
        await _page.WaitForURLAsync("**/Customers/Details/**");
    }
}
