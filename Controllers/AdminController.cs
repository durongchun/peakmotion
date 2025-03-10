using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class AdminController : Controller
    {
        private readonly PeakmotionContext _context;
        private readonly ProductRepo _productRepo;
        private readonly OrderRepo _orderRepo;
        private readonly PmuserRepo _pmuserRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            PeakmotionContext context,
            ProductRepo productRepo,
            OrderRepo orderRepo,
            PmuserRepo pmuserRepo,
            UserManager<IdentityUser> userManager
        )
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

        public IActionResult Products()
        {
            IEnumerable<ProductVM> products = _productRepo.GetAllProducts("ID");
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
            await _productRepo.UploadImagesFromAdminProductEdit(model, NewImages);

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
            return RedirectToAction("Products", new { message = returnMessage });
        }

        public IActionResult Employees(string message = "")
        {
            IEnumerable<UserVM> employees = _pmuserRepo.GetAllEmployees();
            ViewBag.RoleSelectList = _pmuserRepo.GetRoleSelectList();
            ViewBag.Message = message;
            return View(employees);
        }

        [HttpPost, ActionName("EditEmployeeRole")]
        public async Task<IActionResult> EditEmployeeRole(string newRole, string userEmail)
        {
            Console.WriteLine($"UPDATING - role: {newRole}, user: {userEmail}");
            (bool result, string returnMessage) = await _pmuserRepo.EditUserRole(newRole, userEmail);
            ViewBag.Message = returnMessage;
            if (result) return Ok(returnMessage);
            return BadRequest(returnMessage);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _orderRepo.GetAllOrders();
            orders = orders.OrderByDescending(o => o.Pkorderid).ToList();
            var orderVMs = new List<OrderVM>();

            foreach (var order in orders)
            {
                decimal subtotal = 0;
                foreach (var op in order.OrderProducts)
                {
                    var product = op.Fkproduct;
                    if (product == null) continue;

                    decimal finalPrice = product.Regularprice;
                    if (product.Fkdiscount != null && product.Fkdiscount.Description == "discount")
                    {
                        decimal discounted = product.Regularprice - product.Fkdiscount.Amount;
                        if (discounted < 0) discounted = 0;
                        finalPrice = discounted;
                    }
                    subtotal += finalPrice * op.Qty;
                }

                decimal gstRate = 0.05m;
                decimal pstRate = 0.07m;
                decimal gstAmount = subtotal * gstRate;
                decimal pstAmount = subtotal * pstRate;
                decimal totalTax = gstAmount + pstAmount;
                decimal grandTotal = subtotal + totalTax;

                var vm = new OrderVM
                {
                    OrderId = order.Pkorderid,
                    customerName = (order.Fkpmuser?.Firstname + " " + order.Fkpmuser?.Lastname) ?? "Unknown",
                    Email = order.Fkpmuser?.Email ?? "No Email",
                    OrderDate = order.Orderdate,
                    ShippingStatus = order.OrderStatuses.LastOrDefault()?.Orderstate ?? "Pending",
                    Total = grandTotal
                };

                orderVMs.Add(vm);
            }

            return View(orderVMs);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateShippingStatus(int orderId, string status)
        {
            try
            {
                var order = await _orderRepo.GetOrderById(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Orders");
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
                TempData["SuccessMessage"] = "Shipping status updated successfully.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to update shipping status: {ex.Message}";
            }
            return RedirectToAction("Orders");
        }

        public IActionResult Create()
        {
            return View();
        }

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

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepo.GetOrderById(id);
            if (order == null)
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

        public async Task<IActionResult> OrdersDetail(int id)
        {
            var order = await _orderRepo.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }

            var items = new List<OrderItemVM>();
            decimal subtotal = 0;

            foreach (var op in order.OrderProducts)
            {
                var product = op.Fkproduct;
                if (product == null) continue;

                decimal finalPrice = product.Regularprice;
                if (product.Fkdiscount != null && product.Fkdiscount.Description == "discount")
                {
                    decimal discounted = product.Regularprice - product.Fkdiscount.Amount;
                    if (discounted < 0) discounted = 0;
                    finalPrice = discounted;
                }

                decimal lineTotal = finalPrice * op.Qty;
                subtotal += lineTotal;

                items.Add(new OrderItemVM
                {
                    ProductName = product.Name ?? "Unknown",
                    Quantity = op.Qty,
                    Unitprice = finalPrice,
                    LineTotal = lineTotal
                });
            }

            decimal gstRate = 0.05m;
            decimal pstRate = 0.07m;
            decimal gstAmount = subtotal * gstRate;
            decimal pstAmount = subtotal * pstRate;
            decimal totalTax = gstAmount + pstAmount;
            decimal grandTotal = subtotal + totalTax;

            var orderVM = new OrderVM
            {
                OrderId = order.Pkorderid,
                customerName = (order.Fkpmuser?.Firstname + " " + order.Fkpmuser?.Lastname) ?? "Unknown",
                Email = order.Fkpmuser?.Email ?? "No Email",
                OrderDate = order.Orderdate,
                ShippedDate = order.Shippeddate,
                DeliveryDate = order.Deliverydate,
                Pptransactionid = order.Pptransactionid,
                ShippingStatus = order.OrderStatuses.LastOrDefault()?.Orderstate ?? "Pending",
                Total = grandTotal,
                Items = items
            };

            return View(orderVM);
        }
    }
}
