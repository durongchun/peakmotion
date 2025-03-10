using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers
{
    [Authorize] // Only logged-in users can access
    public class OrderController : Controller
    {
        private readonly OrderRepo _orderRepo;
        private readonly ProductRepo _productRepo;
        private readonly PmuserRepo _pmuserRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(OrderRepo orderRepo, ProductRepo productRepo, PmuserRepo pmuserRepo, UserManager<IdentityUser> userManager)
        {
            _orderRepo = orderRepo;
            _pmuserRepo = pmuserRepo;
            _userManager = userManager;
            _productRepo = productRepo;
        }

        // GET: /Order/Index
        public async Task<IActionResult> Index()
        {
            // Get the logged-in IdentityUser
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return View(Enumerable.Empty<OrderVM>());
            }

            // Get pmUserId from Pmuser table using the user's email
            int? pmUserId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);
            if (pmUserId == null || pmUserId == 0)
            {
                return View(Enumerable.Empty<OrderVM>());
            }

            // Retrieve orders belonging to this user (only the user's orders)
            var orders = await _orderRepo.GetOrdersByPmUserId(pmUserId.Value);

            // Map orders to OrderVM
            var orderVMs = new List<OrderVM>();
            foreach (var order in orders)
            {
                // Calculate net total (like Admin does)
                decimal subtotal = 0;
                foreach (var op in order.OrderProducts)
                {
                    var product = op.Fkproduct;
                    if (product == null) continue;

                    decimal finalPrice = product.Regularprice;
                    decimal? discountPrice = _productRepo.calculateProductPriceIfDiscount(product);

                    if (discountPrice.HasValue)
                    {
                        finalPrice = discountPrice.Value;
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
                    OrderDate = order.Orderdate,
                    ShippedDate = order.Shippeddate,
                    DeliveryDate = order.Deliverydate,
                    Pptransactionid = order.Pptransactionid,
                    ShippingStatus = order.OrderStatuses
                        .OrderByDescending(s => s.Pkorderstatusid)
                        .FirstOrDefault()?.Orderstate ?? "Pending",
                    Total = grandTotal
                };

                orderVMs.Add(vm);
            }

            return View(orderVMs);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return NotFound();
            }

            int? pmUserId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);
            if (pmUserId == null || pmUserId == 0)
            {
                return NotFound();
            }

            // Retrieve the order only if it belongs to the logged-in user
            var order = await _orderRepo.GetOrderByIdForUser(id, pmUserId.Value);
            if (order == null)
            {
                return NotFound();
            }

            // Calculate net total (like Admin does)
            decimal subtotalDetail = 0;
            foreach (var op in order.OrderProducts)
            {
                var product = op.Fkproduct;
                if (product == null) continue;

                decimal? discountPrice = _productRepo.calculateProductPriceIfDiscount(product);
                decimal finalPrice = discountPrice ?? product.Regularprice;
                subtotalDetail += finalPrice * op.Qty;
            }

            decimal gstRateDetail = 0.05m;
            decimal pstRateDetail = 0.07m;
            decimal gstAmountDetail = subtotalDetail * gstRateDetail;
            decimal pstAmountDetail = subtotalDetail * pstRateDetail;
            decimal totalTaxDetail = gstAmountDetail + pstAmountDetail;
            decimal grandTotalDetail = subtotalDetail + totalTaxDetail;

            var orderVM = new OrderVM
            {
                OrderId = order.Pkorderid,
                OrderDate = order.Orderdate,
                ShippedDate = order.Shippeddate,
                DeliveryDate = order.Deliverydate,
                Total = grandTotalDetail,
                ShippingStatus = order.OrderStatuses
                    .OrderByDescending(s => s.Pkorderstatusid)
                    .FirstOrDefault()?.Orderstate ?? "Pending",
                Pptransactionid = order.Pptransactionid,
                Items = order.OrderProducts.Select(op =>
                {
                    var p = op.Fkproduct;
                    decimal? discountPrice = _productRepo.calculateProductPriceIfDiscount(p);
                    decimal itemFinalPrice = discountPrice ?? p.Regularprice;

                    return new OrderItemVM
                    {
                        ProductName = p?.Name ?? "Unknown",
                        Quantity = op.Qty,
                        Unitprice = itemFinalPrice,
                        LineTotal = itemFinalPrice * op.Qty
                    };
                }).ToList()
            };

            return View(orderVM);
        }
    }
}
