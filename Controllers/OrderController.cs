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
        private readonly PmuserRepo _pmuserRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(OrderRepo orderRepo, PmuserRepo pmuserRepo, UserManager<IdentityUser> userManager)
        {
            _orderRepo = orderRepo;
            _pmuserRepo = pmuserRepo;
            _userManager = userManager;
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
            var orderVMs = orders.Select(o => new OrderVM
            {
                OrderId = o.Pkorderid,
                OrderDate = o.Orderdate,
                ShippedDate = o.Shippeddate,
                DeliveryDate = o.Deliverydate,
                Pptransactionid = o.Pptransactionid,
                Total = o.OrderProducts.Sum(op => op.Unitprice * op.Qty),
                ShippingStatus = o.OrderStatuses.OrderByDescending(s => s.Pkorderstatusid).FirstOrDefault()?.Orderstate ?? "Pending"
            }).ToList();

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

            var orderVM = new OrderVM
            {
                OrderId = order.Pkorderid,
                OrderDate = order.Orderdate,
                ShippedDate = order.Shippeddate,
                DeliveryDate = order.Deliverydate,
                Total = order.OrderProducts.Sum(op => op.Unitprice * op.Qty),
                ShippingStatus = order.OrderStatuses.OrderByDescending(s => s.Pkorderstatusid).FirstOrDefault()?.Orderstate ?? "Pending",
                Pptransactionid = order.Pptransactionid,
                Items = order.OrderProducts.Select(op => new OrderItemVM
                {
                    ProductName = op.Fkproduct?.Name ?? "Unknown",
                    Quantity = op.Qty,
                    Unitprice = op.Unitprice,
                    LineTotal = op.Unitprice * op.Qty
                }).ToList()
            };

            return View(orderVM);
        }
    }
}
