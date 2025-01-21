using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

public class ProductController : Controller
{
    private readonly ProductRepo _productRepo;

    public ProductController(ProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    // NOTE: 
    // - ATM using Shop/Index for product list 
    // - I'll switch to Product/Index later
    // - (I think that follows previous hw better - ie. Employee, Bank, etc hw)
    // 
    // public IActionResult Index()
    // {
    //     IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
    //     return View(products);
    // }
}