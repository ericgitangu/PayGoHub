using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Common;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.DTOs.M2M;
using PayGoHub.Application.DTOs.Tokens;
using PayGoHub.Application.Interfaces;
using PayGoHub.Web.ViewModels;

namespace PayGoHub.Web.Controllers;

[Authorize]
public class DevicesController : Controller
{
    private readonly IDeviceService _deviceService;
    private readonly ITokenGenerationService _tokenService;
    private readonly IM2MCommandService _m2mService;
    private readonly IActivityLogService _activityLog;
    private const int DefaultPageSize = 10;

    public DevicesController(
        IDeviceService deviceService,
        ITokenGenerationService tokenService,
        IM2MCommandService m2mService,
        IActivityLogService activityLog)
    {
        _deviceService = deviceService;
        _tokenService = tokenService;
        _m2mService = m2mService;
        _activityLog = activityLog;
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

        ViewData["ActivePage"] = "Devices";

        var viewModel = new DeviceDetailsViewModel
        {
            Device = device,
            RecentCommands = await _m2mService.GetRecentCommandsAsync(device.SerialNumber, 5)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateToken(Guid id, string command, string payload, string? secret)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
            return NotFound();

        var request = new TokenGenerationRequestDto
        {
            Device = device.SerialNumber,
            Command = command,
            Payload = payload,
            Secret = secret ?? string.Empty
        };

        var result = await _tokenService.GenerateStatelessAsync(request);

        TempData["TokenResult"] = result.Status == "ok" ? result.Token : null;
        TempData["TokenError"] = result.Status == "error" ? result.Error : null;
        TempData["TokenCommand"] = command;

        // Log activity
        var commandLabel = DeviceDetailsViewModel.TokenCommands.GetValueOrDefault(command, command);
        if (result.Status == "ok")
        {
            await _activityLog.LogAsync(
                "token_generated",
                $"Token Generated: {commandLabel}",
                $"Generated {commandLabel} token for device {device.SerialNumber}",
                "Device", device.Id, device.SerialNumber,
                "success", User.Identity?.Name,
                "bi-key-fill", "success",
                new { command, payload, token_prefix = result.Token?[..Math.Min(6, result.Token?.Length ?? 0)] }
            );
        }
        else
        {
            await _activityLog.LogAsync(
                "token_generation_failed",
                $"Token Generation Failed: {commandLabel}",
                $"Failed to generate token for device {device.SerialNumber}: {result.Error}",
                "Device", device.Id, device.SerialNumber,
                "failed", User.Identity?.Name,
                "bi-x-circle-fill", "danger",
                new { command, payload, error = result.Error }
            );
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendCommand(Guid id, string commandName, string? unlockCode, string? callbackUrl)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
            return NotFound();

        var request = new CommandRequestDto
        {
            Identifier = new IdentifierDto { Kind = "serial", Value = device.SerialNumber },
            Command = new CommandDetailDto
            {
                Name = commandName,
                Details = !string.IsNullOrEmpty(unlockCode)
                    ? new Dictionary<string, object> { { "unlock_code", unlockCode } }
                    : new Dictionary<string, object>()
            },
            CallbackUrl = callbackUrl ?? string.Empty
        };

        var result = await _m2mService.CreateCommandAsync(request);

        TempData["CommandResult"] = result.Status;
        TempData["CommandId"] = result.CommandId;
        TempData["CommandName"] = commandName;

        // Log activity
        var commandLabel = DeviceDetailsViewModel.M2MCommands.GetValueOrDefault(commandName, commandName);
        await _activityLog.LogAsync(
            "m2m_command_sent",
            $"M2M Command: {commandLabel}",
            $"Sent {commandLabel} command to device {device.SerialNumber}",
            "Device", device.Id, device.SerialNumber,
            result.Status == "pending" ? "pending" : "success",
            User.Identity?.Name,
            "bi-broadcast", "warning",
            new { commandName, commandId = result.CommandId, status = result.Status }
        );

        return RedirectToAction(nameof(Details), new { id });
    }
}
