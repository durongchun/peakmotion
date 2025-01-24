using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;

namespace peakmotion.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SessionRepo _sessionRepo;

    

    public HomeController(ILogger<HomeController> logger, SessionRepo sessionRepo)
    {
        _logger = logger;
        _sessionRepo = sessionRepo;
    }

    public IActionResult Index()
    {
        _sessionRepo.AddSession();
        return View();
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
}
