using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

[Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly ProductRepo _productRepo;
    private readonly PmuserRepo _pmuserRepo;

    public AdminController(ProductRepo productRepo, PmuserRepo pmuserRepo)
    {
        _productRepo = productRepo;
        _pmuserRepo = pmuserRepo;
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
}