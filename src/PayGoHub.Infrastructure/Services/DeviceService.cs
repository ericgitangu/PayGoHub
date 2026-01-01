using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

public class DeviceService : IDeviceService
{
    private readonly PayGoHubDbContext _context;

    public DeviceService(PayGoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeviceDto>> GetAllAsync()
    {
        return await _context.Devices
            .Include(d => d.Installation)
            .ThenInclude(i => i!.Customer)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DeviceDto
            {
                Id = d.Id,
                SerialNumber = d.SerialNumber,
                Model = d.Model,
                Type = d.Type.ToString(),
                Status = d.Status.ToString(),
                StatusClass = GetStatusClass(d.Status),
                BatteryHealth = d.BatteryHealth,
                LastSyncDate = d.LastSyncDate,
                CustomerName = d.Installation != null ? d.Installation.Customer.FirstName + " " + d.Installation.Customer.LastName : null
            })
            .ToListAsync();
    }

    public async Task<DeviceDto?> GetByIdAsync(Guid id)
    {
        var device = await _context.Devices
            .Include(d => d.Installation)
            .ThenInclude(i => i!.Customer)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
            return null;

        return new DeviceDto
        {
            Id = device.Id,
            SerialNumber = device.SerialNumber,
            Model = device.Model,
            Type = device.Type.ToString(),
            Status = device.Status.ToString(),
            StatusClass = GetStatusClass(device.Status),
            BatteryHealth = device.BatteryHealth,
            LastSyncDate = device.LastSyncDate,
            CustomerName = device.Installation?.Customer?.FullName,
            CreatedAt = device.CreatedAt
        };
    }

    public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            SerialNumber = dto.SerialNumber,
            Model = dto.Model,
            Status = DeviceStatus.Active,
            BatteryHealth = 100
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return new DeviceDto
        {
            Id = device.Id,
            SerialNumber = device.SerialNumber,
            Model = device.Model,
            Type = device.Type.ToString(),
            Status = device.Status.ToString(),
            StatusClass = GetStatusClass(device.Status),
            BatteryHealth = device.BatteryHealth
        };
    }

    private static string GetStatusClass(DeviceStatus status) => status switch
    {
        DeviceStatus.Active => "success",
        DeviceStatus.Inactive => "secondary",
        DeviceStatus.Faulty => "danger",
        DeviceStatus.Replaced => "info",
        _ => "secondary"
    };
}
