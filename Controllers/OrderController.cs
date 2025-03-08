
using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Controllers;

public class OrderController : BaseController
{
    private readonly PeakmotionContext _context;
    private readonly CookieRepo _cookieRepo;
    private readonly OrderRepo _orderRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public OrderController(PeakmotionContext context, CookieRepo cookieRepo, OrderRepo orderRepo, UserManager<IdentityUser> userManager): base(cookieRepo)
    {
        _context = context;
        _orderRepo = orderRepo;
        _userManager = userManager;
    }

    // Get all orders for the logged-in user
    public async Task<IActionResult> Index()
    {
        // Get the current user's ID
        var userId = _userManager.GetUserId(User);  // Corrected line

        // Fetch the user's orders from the repository
        var orders = await _orderRepo.GetOrdersByUserId(userId);

        // Map Order models to OrderVM for the view
        var orderVM = orders.Select(order => new OrderVM
        {
            OrderDate = order.Orderdate,
            OrderId = order.Pkorderid,
        }).ToList();

        // Return the view with the list of orders
        return View(orderVM);  // Pass the OrderVM list to the view
    }

    // Get details of a specific order by its ID
    public async Task<IActionResult> DetailsOrderId(int id)
    {
        var userId = _userManager.GetUserId(User);  // Get the current user's ID
        var order = await _orderRepo.GetOrderByIdForUser(id, userId);

        if (order == null)
        {
            return NotFound();  // Return NotFound if the order doesn't exist
        }

        // Create an OrderVM to pass to the view
        var orderVM = new OrderVM
        {
            OrderId = order.Pkorderid,
            OrderDate = order.Orderdate,
            // Add other properties as needed
        };

        // Return the details view with the OrderVM
        return View(orderVM);
    }
}
