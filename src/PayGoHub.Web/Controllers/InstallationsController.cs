using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class InstallationsController : Controller
{
    private readonly IInstallationService _installationService;

    public InstallationsController(IInstallationService installationService)
    {
        _installationService = installationService;
    }

    public async Task<IActionResult> Index()
    {
        var installations = await _installationService.GetAllAsync();
        return View(installations);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var installation = await _installationService.GetByIdAsync(id);
        if (installation == null)
            return NotFound();

        return View(installation);
    }
}
