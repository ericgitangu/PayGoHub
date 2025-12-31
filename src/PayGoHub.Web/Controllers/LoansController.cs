using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class LoansController : Controller
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    public async Task<IActionResult> Index()
    {
        var loans = await _loanService.GetAllAsync();
        return View(loans);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        if (loan == null)
            return NotFound();

        return View(loan);
    }
}
