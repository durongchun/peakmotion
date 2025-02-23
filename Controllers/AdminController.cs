using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

[Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly ProductRepo _productRepo;
    private readonly PmuserRepo _pmuserRepo;
    private readonly UserManager<IdentityUser> _userManager;
    public AdminController(ProductRepo productRepo, PmuserRepo pmuserRepo, UserManager<IdentityUser> userManager)
    {
        _productRepo = productRepo;
        _pmuserRepo = pmuserRepo;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Products()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

    public IActionResult Employees()
    {
        IEnumerable<UserVM> employees = _pmuserRepo.GetAllEmployees();
        ViewBag.RoleSelectList = _pmuserRepo.GetRoleSelectList();
        return View(employees);
    }


    [HttpPost]
    public async Task<IActionResult> EditEmployeeRole(string newRole)
    {
        Console.WriteLine($"role: {newRole}");
        var result = await _pmuserRepo.EditUserRole(newRole);
        if (!result) ViewBag.Message = "Unable to update the user's role";
        return RedirectToAction("Employees", new { message = "Successfully updated role" });
    }
}