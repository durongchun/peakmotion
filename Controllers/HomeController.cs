using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

public class HomeController : Controller
{
    private readonly ProductRepo _productRepo;
    private readonly ILogger<HomeController> _logger;
    public HomeController(ProductRepo productRepo, ILogger<HomeController> logger)
    {
        _productRepo = productRepo;
        _logger = logger;
    }

    public IActionResult Index()
    {
        IEnumerable<ProductVM> featuredProducts = _productRepo.GetTopFeaturedProductsBy(10);
        Console.WriteLine($"Featured Products: {featuredProducts.Count()}");
        return View(featuredProducts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
