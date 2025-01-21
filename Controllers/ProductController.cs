using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

public class ProductController : Controller
{
    private readonly ProductRepository _productRepo;

    public ProductController(ProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public IActionResult Index()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }
}