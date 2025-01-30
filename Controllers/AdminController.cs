using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

[Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly ProductRepo _productRepo;

    public AdminController(ProductRepo productRepo)
    {
        _productRepo = productRepo;
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
}