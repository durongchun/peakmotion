
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
    private readonly PmuserRepo _pmuserRepo;

    public OrderController(PeakmotionContext context, OrderRepo orderRepo, UserManager<IdentityUser> userManager, PmuserRepo pmuserRepo)
    {
        _context = context;
        _orderRepo = orderRepo;
        _userManager = userManager;
        _pmuserRepo = pmuserRepo;
    }

    // Get all orders for the logged-in user
    public async Task<IActionResult> Index()
    {
        // Get the current user's ID
        var userId = _pmuserRepo.GetUserId(); ;  // Corrected line

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
    public async Task<IActionResult> Details(int id)
    {
        var userId = _pmuserRepo.GetUserId();  // Get the current user's ID
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
            Total = 100,
            orderStatuses = order.OrderStatuses.Where((os) => os.Fkorderid == id)
            .Select(os => new OrderStatusVM
            {
                Status = os.Orderstate,

            }).ToList(),
            OrderProducts = order.OrderProducts.Where((op) => op.Fkorderid == id).Select(op => new OrderProductVM
            {
                ProductId = op.Fkproductid,
                Quantity = op.Qty,
                Unitprice = op.Unitprice,
                ProductName = op.Pkorderproductid.ToString(),
            }).ToList()
        };

        // Return the details view with the OrderVM
        return View(orderVM);
    }
}
