using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using System.Linq;

namespace peakmotion.Controllers;

[Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly PeakmotionContext _context;
    private readonly ProductRepo _productRepo;
    private readonly OrderRepo _orderRepo;
    private readonly PmuserRepo _pmuserRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(PeakmotionContext context, ProductRepo productRepo,
    OrderRepo orderRepo, PmuserRepo pmuserRepo, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _pmuserRepo = pmuserRepo;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }


    // Products View
    public IActionResult Products()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

    public IActionResult ProductEdit(int id)
    {
        ProductVM? product = _productRepo.GetProduct(id);

        return View(product);
    }

    [HttpPost("Product/Edit")]
    public async Task<IActionResult> ProductDetailsEdit(ProductVM model, List<IFormFile> NewImages)
    {
        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                foreach (var error in ModelState[key].Errors)
                {
                    Console.WriteLine($"Error with key '{key}': {error.ErrorMessage}");
                }
            }
            return View(model);
        }

        var product = await _context.Products.FindAsync(model.ID);
        if (product == null)
        {
            return NotFound();
        }

        await _productRepo.UpdateProductDetail(product, model);


        await _productRepo.UpdateProductCategoriesAsync(model);


        // Handle new images
        await _productRepo.UploadImagesFromAdminProductEdit(model, NewImages);


        // Redirect to the product edit page
        return RedirectToAction("ProductEdit", new { id = model.ID });
    }




    public IActionResult ProductDelete(int id)
    {
        ProductVM? product = _productRepo.GetProduct(id);

        return View(product);
    }

    [HttpPost, ActionName("ProductDelete")]
    public IActionResult ProductDeleteConfirmed(int id)
    {
        string returnMessage = _productRepo.RemoveProduct(id);

        return RedirectToAction("Products",
                                new { message = returnMessage });

    }

    /// <summary>
    ///     List All Users (Employee, Customers, Admin)
    /// </summary>
    /// <returns></returns>
    public IActionResult Employees(string message = "")
    {
        IEnumerable<UserVM> employees = _pmuserRepo.GetAllEmployees();
        ViewBag.RoleSelectList = _pmuserRepo.GetRoleSelectList();
        ViewBag.Message = message;
        return View(employees);
    }

    /// <summary>
    ///     Edit a user's role
    /// </summary>
    /// <param name="newRole"></param>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    [HttpPost, ActionName("EditEmployeeRole")]
    public async Task<IActionResult> EditEmployeeRole(string newRole, string userEmail)
    {
        Console.WriteLine($"UPDATING - role: {newRole}, user: {userEmail}");
        (bool result, string returnMessage) = await _pmuserRepo.EditUserRole(newRole, userEmail);
        ViewBag.Message = returnMessage;
        if (result) return Ok(returnMessage);
        return BadRequest(returnMessage);
    }

    /// <summary>
    ///     Delete the user's role
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("DeleteUserRole")]
    public async Task<IActionResult> DeleteUserRole(string newRole, string userEmail)
    {
        Console.WriteLine($"DELETING - role: {newRole}, user: {userEmail}");
        (bool result, string returnMessage) = await _pmuserRepo.DeleteUserRole(newRole, userEmail);

        return RedirectToAction("Employees", new { message = returnMessage });
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
            ShippingStatus = order.OrderStatuses.LastOrDefault()?.Orderstate ?? "Pending"
        }).ToList();

        var shippingStatus = new List<String> { "Pending", "Shipped", "Delivered", "Cancelled" };
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

}

