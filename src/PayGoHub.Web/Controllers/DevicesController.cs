using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class DevicesController : Controller
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task<IActionResult> Index()
    {
        var devices = await _deviceService.GetAllAsync();
        return View(devices);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
            return NotFound();

        return View(device);
    }
}
