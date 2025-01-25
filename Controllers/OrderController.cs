using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using SQLitePCL;

namespace peakmotion.Controllers;
public class OrderController: Controller
{
    private readonly OrderRepo _orderRepo;
    public OrderController (OrderRepo orderRepo)
    {
        _orderRepo = orderRepo;
    }
    
    public async Task<IActionResult> Index()
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
        return View(OrderVM);
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

