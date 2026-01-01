using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Interfaces;
using PayGoHub.Web.Models;

namespace PayGoHub.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _dashboardService.GetDashboardDataAsync();
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Settings()
    {
        ViewData["ActivePage"] = "Settings";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
