using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

// [Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly PeakmotionContext _context;
    private readonly ProductRepo _productRepo;
    private readonly OrderRepo _orderRepo;

    public AdminController(PeakmotionContext context,ProductRepo productRepo, OrderRepo orderRepo)
    {
        _context = context;
        _productRepo = productRepo;
        _orderRepo = orderRepo;
    }

    public IActionResult Products()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }
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
        var shippingStatus =  new List<String>{"Pending","Shipped","Delivered","Cancelled"};
        ViewBag.shippingStatus = shippingStatus;
        
        return View(OrderVM);
    }




    [HttpPost]
    public async Task<IActionResult> UpdateShippingStatus(int orderId, string status)
    {
        try
        {
            Console.WriteLine($"Received request to update status for Order ID: {orderId} to {status}");

            // Find the order by its ID
            var order = await _orderRepo.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Update the shipping status for this order
            var currentStatus = order.OrderStatuses.LastOrDefault();
            if (currentStatus != null)
            {
                currentStatus.Orderstate = status;  // Update the existing status
            }
            else
            {
                // If no previous status exists, create a new status entry
                order.OrderStatuses.Add(new OrderStatus
                {
                    Orderstate = status,
                    Fkorderid = order.Pkorderid
                });
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Log successful status update
            Console.WriteLine($"Successfully updated status for Order ID: {orderId} to {status}");

            return RedirectToAction("Orders");  // Redirect back to the orders page
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.WriteLine($"Error updating status for Order ID {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }



    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> CreateConfirmed(OrderCreateVM orderVM)
    {
        if(ModelState.IsValid)
        {
            var order = new Order{
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
    public async Task<IActionResult> Edit(int id)
    { var order = await _orderRepo.GetOrderById(id);
    if(order == null)
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
    [HttpPost]
    public async Task<IActionResult> EditConfirmed(int id, OrderCreateVM orderVM)
    {
        if(ModelState.IsValid)
        {
            var order = await _orderRepo.GetOrderById(id);
            if(order == null)
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
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _orderRepo.GetOrderById(id);
        if(order == null)
        {
            return NotFound();
        }
        return View(order);
    }
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _orderRepo.DeleteOrder(id);
        return RedirectToAction("Index");
    }

  

}