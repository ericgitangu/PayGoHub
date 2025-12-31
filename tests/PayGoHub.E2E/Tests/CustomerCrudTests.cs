using PayGoHub.E2E.PageObjects;

namespace PayGoHub.E2E.Tests;

/// <summary>
/// E2E tests for Customer CRUD operations.
/// Run with: dotnet test tests/PayGoHub.E2E --filter "FullyQualifiedName~CustomerCrudTests"
/// </summary>
[TestFixture]
[Category("E2E")]
public class CustomerCrudTests : E2ETestBase
{
    private CustomersPage _customers = null!;

    [SetUp]
    public void Setup()
    {
        _customers = new CustomersPage(Page);
    }

    [Test]
    public async Task CustomersList_ShouldDisplayCustomers()
    {
        await _customers.NavigateAsync();

        await Expect(_customers.PageTitle).ToBeVisibleAsync();
        await Expect(_customers.CustomerTable).ToBeVisibleAsync();
    }

    [Test]
    public async Task CustomersList_ShouldHaveSeededData()
    {
        await _customers.NavigateAsync();

        var count = await _customers.GetCustomerCountAsync();

        Assert.That(count, Is.GreaterThan(0), "Should have seeded customers");
    }

    [Test]
    public async Task CreateCustomer_ShouldDisplayForm()
    {
        await _customers.NavigateToCreateAsync();

        await Expect(_customers.FirstNameInput).ToBeVisibleAsync();
        await Expect(_customers.LastNameInput).ToBeVisibleAsync();
        await Expect(_customers.EmailInput).ToBeVisibleAsync();
        await Expect(_customers.PhoneNumberInput).ToBeVisibleAsync();
        await Expect(_customers.RegionInput).ToBeVisibleAsync();
        await Expect(_customers.DistrictInput).ToBeVisibleAsync();
        await Expect(_customers.AddressInput).ToBeVisibleAsync();
        await Expect(_customers.SubmitButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateCustomer_ShouldCreateNewCustomer()
    {
        var uniqueName = $"TestUser_{DateTime.UtcNow.Ticks}";

        await _customers.NavigateToCreateAsync();

        await _customers.FillCustomerFormAsync(
            firstName: uniqueName,
            lastName: "E2ETest",
            email: $"{uniqueName.ToLower()}@test.com",
            phoneNumber: "+254700000001",
            region: "Nairobi",
            district: "Westlands",
            address: "123 E2E Test Street"
        );

        await _customers.SubmitFormAsync();

        // Verify redirect to list
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers.*"));

        // Verify customer exists
        var exists = await _customers.CustomerExistsAsync(uniqueName);
        Assert.That(exists, Is.True, $"Customer {uniqueName} should exist after creation");
    }

    [Test]
    public async Task CreateCustomer_CancelButton_ShouldReturnToList()
    {
        await _customers.NavigateToCreateAsync();

        await _customers.CancelButton.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers.*"));
    }

    [Test]
    public async Task CustomerDetails_ShouldDisplayCustomerInfo()
    {
        await _customers.NavigateAsync();

        // Click on first customer details - uses icon button (bi-eye)
        var detailsLink = Page.Locator("table.table tbody tr").First.Locator("a[href*='Details']");
        await detailsLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers/Details.*"));
    }

    [Test]
    public async Task EditCustomer_ShouldUpdateCustomer()
    {
        await _customers.NavigateAsync();

        // Get the first customer's edit link - uses icon button (bi-pencil)
        var editLink = Page.Locator("table.table tbody tr").First.Locator("a[href*='Edit']");
        await editLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers/Edit.*"));

        // Verify form is pre-filled
        await Expect(_customers.FirstNameInput).Not.ToHaveValueAsync("");

        // Make a small change
        var originalAddress = await _customers.AddressInput.InputValueAsync();
        await _customers.AddressInput.FillAsync(originalAddress + " (Updated)");

        await _customers.SubmitFormAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Customers.*"));
    }
}
