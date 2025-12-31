using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Common;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class DevicesController : Controller
{
    private readonly IDeviceService _deviceService;
    private const int DefaultPageSize = 10;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize, string? search = null)
    {
        ViewData["ActivePage"] = "Devices";
        ViewData["CurrentSearch"] = search;

        var allDevices = await _deviceService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            allDevices = allDevices.Where(d =>
                (d.SerialNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Model?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Type?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Status?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var paginatedList = PaginatedList<DeviceDto>.Create(allDevices, page, pageSize);
        return View(paginatedList);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
            return NotFound();

        return View(device);
    }
}
