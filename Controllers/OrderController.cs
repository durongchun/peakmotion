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

public class OrderController : Controller
{
    private readonly PeakmotionContext _context;
    private readonly OrderRepo _orderRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public OrderController(PeakmotionContext context, OrderRepo orderRepo, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _orderRepo = orderRepo;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = 1;
        var orders = await _orderRepo.GetOrdersByUserId(userId);

        // Map Order models to OrderVM
        var orderVM = orders.Select(order => new OrderVM
        {
            OrderDate = order.Orderdate,
            OrderId = order.Pkorderid,
        }).ToList();

        return View(orderVM);  // Pass the OrderVM list to the view
    }


    public async Task<IActionResult> DetailsOrderId(int id)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _orderRepo.GetOrderByIdForUser(id, userId);

        if (order == null)
        {
            return NotFound();
        }
        return View(order);
    }
}