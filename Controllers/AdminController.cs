using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using System.Linq;

namespace peakmotion.Controllers;

//[Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly PeakmotionContext _context;
    private readonly ProductRepo _productRepo;
    private readonly OrderRepo _orderRepo;
    private readonly PmuserRepo _pmuserRepo;

    public AdminController(PeakmotionContext context, ProductRepo productRepo, OrderRepo orderRepo, PmuserRepo pmuserRepo)
    {
        _context = context;
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _pmuserRepo = pmuserRepo;
    }

    // Products View
    public IActionResult Products()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

    // Employees View
    public IActionResult Employees()
    {
        IEnumerable<UserVM> employees = _pmuserRepo.GetAllEmployees();
        ViewBag.RoleSelectList = _pmuserRepo.GetRoleSelectList();
        return View(employees);
    }

    // Orders View
    public async Task<IActionResult> Orders()
    {
        var orders = await _orderRepo.GetAllOrders();
        var OrderVM = orders.Select(order => new OrderVM
        {
            OrderId = order.Pkorderid,
            customerName = order.Fkpmuser?.Firstname ?? "Unknown",
            OrderDate = order.Orderdate,
            Total = order.OrderProducts.Sum(op => op.Unitprice * op.Qty),
            ShippingStatus = order.OrderStatuses.LastOrDefault()?.Orderstate ?? "Unknown"
        }).ToList();
        
        var shippingStatus = new List<String> { "Pending", "Shipped", "Delivered", "Cancelled" };
        ViewBag.shippingStatus = shippingStatus;
        
        return View(OrderVM);
    }

    // Create Order View
    public IActionResult Create()
    {
        return View();
    }

    // Create Order Logic
    [HttpPost]
    public async Task<IActionResult> CreateConfirmed(OrderCreateVM orderVM)
    {
        if (ModelState.IsValid)
        {
            var order = new Order
            {
                Orderdate = orderVM.OrderDate,
                Fkpmuserid = orderVM.Fkpmuserid,
                OrderProducts = orderVM.OrderProducts.Select(op => new OrderProduct
                {
                    Fkproductid = op.ProductId,
                    Qty = op.Quantity,
                    Unitprice = op.Unitprice
                }).ToList()
            };
            
            var initialStatus = new OrderStatus
            {
                Orderstate = "Pending",
                Fkorderid = order.Pkorderid
            };

            order.OrderStatuses = new List<OrderStatus> { initialStatus };
            await _orderRepo.CreateOrder(order);
            return RedirectToAction("Index");
        }
        return View(orderVM);
    }

    // Edit Order View
    public async Task<IActionResult> Edit(int id)
    {
        var order = await _orderRepo.GetOrderById(id);
        if (order == null)
        {
            return NotFound();
        }

        var orderVM = new OrderCreateVM
        {
            OrderDate = order.Orderdate,
            Fkpmuserid = order.Fkpmuserid ?? 0,
            OrderProducts = order.OrderProducts.Select(op => new OrderProductVM
            {
                ProductId = op.Fkproductid,
                Quantity = op.Qty,
                Unitprice = op.Unitprice
            }).ToList()
        };

        return View(orderVM);
    }

    // Update Order Logic
    [HttpPost]
    public async Task<IActionResult> EditConfirmed(int id, OrderCreateVM orderVM)
    {
        if (ModelState.IsValid)
        {
            var order = await _orderRepo.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Orderdate = orderVM.OrderDate;
            order.Fkpmuserid = orderVM.Fkpmuserid;
            order.OrderProducts = orderVM.OrderProducts.Select(op => new OrderProduct
            {
                Fkproductid = op.ProductId,
                Qty = op.Quantity,
                Unitprice = op.Unitprice
            }).ToList();
            await _orderRepo.UpdateOrder(order);
            return RedirectToAction("Index");
        }
        return View(orderVM);
    }

    // Delete Order View
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _orderRepo.GetOrderById(id);
        if (order == null)
        {
            return NotFound();
        }
        return View(order);
    }

    // Confirm Order Deletion
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _orderRepo.DeleteOrder(id);
        return RedirectToAction("Index");
    }

    // Update Shipping Status Logic
    [HttpPost]
    public async Task<IActionResult> UpdateShippingStatus(int orderId, string status)
    {
        try
        {
            var order = await _orderRepo.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var currentStatus = order.OrderStatuses.LastOrDefault();
            if (currentStatus != null)
            {
                currentStatus.Orderstate = status;
            }
            else
            {
                order.OrderStatuses.Add(new OrderStatus
                {
                    Orderstate = status,
                    Fkorderid = order.Pkorderid
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Orders");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }
}
