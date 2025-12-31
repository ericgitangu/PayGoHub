using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PayGoHub.Application.DTOs.Tokens;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Services;

namespace PayGoHub.Tests.Unit;

[Trait("Category", "Unit")]
public class TokenGenerationServiceTests
{
    private readonly PayGoHubDbContext _dbContext;
    private readonly TokenGenerationService _service;

    public TokenGenerationServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayGoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PayGoHubDbContext(options);
        var logger = Mock.Of<ILogger<TokenGenerationService>>();
        _service = new TokenGenerationService(_dbContext, logger);
    }

    [Fact]
    public async Task GenerateStatelessAsync_WithoutSecret_ReturnsError()
    {
        // Arrange
        var request = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "" // No secret
        };

        // Act
        var result = await _service.GenerateStatelessAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Contain("secret");
    }

    [Fact]
    public async Task GenerateStatelessAsync_WithValidRequest_ReturnsToken()
    {
        // Arrange
        var request = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        // Act
        var result = await _service.GenerateStatelessAsync(request);

        // Assert
        result.Status.Should().Be("ok");
        result.Token.Should().NotBeNullOrEmpty();
        result.SequenceNumber.Should().Be(42);
    }

    [Fact]
    public async Task GenerateStatelessAsync_WithHexEncoding_ReturnsHexToken()
    {
        // Arrange
        var request = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF",
            Encoding = "hex"
        };

        // Act
        var result = await _service.GenerateStatelessAsync(request);

        // Assert
        result.Status.Should().Be("ok");
        result.Token.Should().MatchRegex("^[0-9A-F]+$");
    }

    [Fact]
    public async Task GenerateStatelessAsync_SameInputs_ReturnsSameToken()
    {
        // Arrange
        var request1 = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        var request2 = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        // Act
        var result1 = await _service.GenerateStatelessAsync(request1);
        var result2 = await _service.GenerateStatelessAsync(request2);

        // Assert
        result1.Token.Should().Be(result2.Token);
    }

    [Fact]
    public async Task GenerateStatelessAsync_DifferentSequence_ReturnsDifferentToken()
    {
        // Arrange
        var request1 = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        var request2 = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 43, // Different sequence
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        // Act
        var result1 = await _service.GenerateStatelessAsync(request1);
        var result2 = await _service.GenerateStatelessAsync(request2);

        // Assert
        result1.Token.Should().NotBe(result2.Token);
    }

    [Fact]
    public async Task GenerateAsync_DeviceNotFound_ReturnsError()
    {
        // Arrange
        var request = new TokenGenerationRequestDto
        {
            Device = "NONEXISTENT",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Be("Device not found");
    }

    [Fact]
    public async Task GenerateAsync_WithDevice_StoresToken()
    {
        // Arrange
        var device = new Device
        {
            SerialNumber = "SCBLNX/A/BT/240300126005",
            Model = "SHS-80W",
            Status = DeviceStatus.Active
        };
        await _dbContext.Devices.AddAsync(device);
        await _dbContext.SaveChangesAsync();

        var request = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 42,
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        result.Status.Should().Be("ok");
        result.Token.Should().NotBeNullOrEmpty();

        // Verify token was stored
        var storedToken = await _dbContext.Tokens.FirstOrDefaultAsync();
        storedToken.Should().NotBeNull();
        storedToken!.DeviceIdentifier.Should().Be("SCBLNX/A/BT/240300126005");
        storedToken.Type.Should().Be(TokenType.Stateful);
    }

    [Fact]
    public async Task GenerateAsync_AutoIncrementSequence_WhenNotProvided()
    {
        // Arrange
        var device = new Device
        {
            SerialNumber = "SCBLNX/A/BT/240300126005",
            Model = "SHS-80W",
            Status = DeviceStatus.Active
        };
        await _dbContext.Devices.AddAsync(device);

        // Add existing token with sequence 10
        await _dbContext.Tokens.AddAsync(new Token
        {
            DeviceIdentifier = "SCBLNX/A/BT/240300126005",
            TokenValue = "1234-5678-9012-3456",
            Command = "unlock_relative",
            SequenceNumber = 10,
            Type = TokenType.Stateful
        });
        await _dbContext.SaveChangesAsync();

        var request = new TokenGenerationRequestDto
        {
            Device = "SCBLNX/A/BT/240300126005",
            Command = "unlock_relative",
            Payload = "30",
            SequenceNumber = 0, // Request auto-increment
            Secret = "0123456789ABCDEF0123456789ABCDEF"
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        result.Status.Should().Be("ok");
        result.SequenceNumber.Should().Be(11); // Auto-incremented
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
