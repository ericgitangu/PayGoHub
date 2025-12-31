using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _customerService.GetAllAsync();
        return View(customers);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        return View(customer);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _customerService.CreateAsync(dto);
        TempData["Success"] = "Customer created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        var updateDto = new UpdateCustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Region = customer.Region,
            District = customer.District,
            Address = customer.Address,
            Status = customer.Status
        };

        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCustomerDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(dto);

        var result = await _customerService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();

        TempData["Success"] = "Customer updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);
        if (!result)
            return NotFound();

        TempData["Success"] = "Customer deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
