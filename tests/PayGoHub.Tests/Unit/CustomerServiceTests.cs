using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Services;

namespace PayGoHub.Tests.Unit;

public class CustomerServiceTests : IDisposable
{
    private readonly PayGoHubDbContext _context;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayGoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PayGoHubDbContext(options);
        _service = new CustomerService(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CreateCustomer("John", "Doe", "john@example.com"),
            CreateCustomer("Jane", "Smith", "jane@example.com")
        };
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.FirstName == "John");
        result.Should().Contain(c => c.FirstName == "Jane");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var customer = CreateCustomer("Peter", "Otieno", "peter@example.com");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Peter");
        result.LastName.Should().Be("Otieno");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCustomer_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidCustomer_CreatesAndReturnsCustomer()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            FirstName = "Mary",
            LastName = "Wanjiku",
            Email = "mary@example.com",
            PhoneNumber = "+254700000000",
            Region = "Nairobi",
            District = "Westlands",
            Address = "123 Test Street"
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be("Mary");

        var savedCustomer = await _context.Customers.FindAsync(result.Id);
        savedCustomer.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_UpdatesCustomer()
    {
        // Arrange
        var customer = CreateCustomer("Original", "Name", "original@example.com");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCustomerDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            PhoneNumber = "+254700000001",
            Region = "Mombasa",
            District = "Nyali",
            Address = "456 New Street",
            Status = "Active"
        };

        // Act
        var result = await _service.UpdateAsync(customer.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task DeleteAsync_ExistingCustomer_SoftDeletesCustomer()
    {
        // Arrange
        var customer = CreateCustomer("ToDelete", "Customer", "delete@example.com");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(customer.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedCustomer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == customer.Id);
        deletedCustomer.Should().NotBeNull();
        deletedCustomer!.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CreateCustomer("A", "Customer", "a@example.com"),
            CreateCustomer("B", "Customer", "b@example.com"),
            CreateCustomer("C", "Customer", "c@example.com")
        };
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GetCountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetNewCustomersThisMonthAsync_ReturnsCorrectCount()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CreateCustomer("New", "Customer1", "new1@example.com"),
            CreateCustomer("New", "Customer2", "new2@example.com")
        };
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GetNewCustomersThisMonthAsync();

        // Assert
        count.Should().Be(2);
    }

    private static Customer CreateCustomer(string firstName, string lastName, string email)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = "+254700000000",
            Region = "Nairobi",
            District = "Westlands",
            Address = "123 Test Street",
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
