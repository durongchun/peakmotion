using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;


namespace peakmotion.Repositories
{
    public class ShopRepo
    {
        private readonly PeakmotionContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PmuserRepo _pmuserRepo;
        private readonly CookieRepo _cookieRepo;
        private readonly ProductRepo _productRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ShopRepo(PeakmotionContext context, UserManager<IdentityUser> userManager, PmuserRepo pmuserRepo, CookieRepo cookieRepo, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _pmuserRepo = pmuserRepo;
            _cookieRepo = cookieRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<ShippingVM> GetShippingInfo()
        {
            var userId = _pmuserRepo.GetUserId();

            return _context.Pmusers
                .Where(user => user.Pkpmuserid == userId) // Filter by logged-in user's ID
                .Select(user => new ShippingVM
                {
                    ID = user.Pkpmuserid,
                    EmailAddress = user.Email,
                    FirstName = user.Firstname,
                    LastName = user.Lastname,
                    PhoneNumber = user.Phone,
                    Address = user.Address,
                    ApptUnit = user.Address,
                    City = user.City,
                    Province = user.Province,
                    PostalCode = user.Postalcode
                })
                .ToList();
        }


        public void SaveShippingInfo(ShippingVM model)
        {
            var country = "CA";

            var userShippingInfo = _context.Pmusers
                .FirstOrDefault(u => u.Email == model.EmailAddress);

            if (userShippingInfo != null)
            {
                // Update existing user properties
                userShippingInfo.Firstname = model.FirstName;
                userShippingInfo.Lastname = model.LastName;
                userShippingInfo.Phone = model.PhoneNumber;
                userShippingInfo.Address = model.Address;
                userShippingInfo.City = model.City;
                userShippingInfo.Province = model.Province;
                userShippingInfo.Postalcode = model.PostalCode;
                userShippingInfo.Country = country;

                // Mark the entity as modified
                _context.Entry(userShippingInfo).State = EntityState.Modified;
            }
            else
            {
                userShippingInfo = new Pmuser
                {
                    Firstname = model.FirstName,
                    Lastname = model.LastName,
                    Phone = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Province = model.Province,
                    Postalcode = model.PostalCode,
                    Country = country,
                    Email = model.EmailAddress,
                };

                _context.Pmusers.Add(userShippingInfo);
            }

            _context.SaveChanges();
        }

        public void SaveOrderInfo(PayPalConfirmationVM model)
        {
            var userId = _pmuserRepo.GetUserId();


            var newOrder = new Order
            {
                Pptransactionid = model.TransactionId,
                Orderdate = DateOnly.FromDateTime(DateTime.Now),
                Fkpmuserid = userId,

            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

        }

        public void SaveOrderStatus(PayPalConfirmationVM model)
        {
            var orderStatus = "Pending";
            var orderId = GetOrderId(model);

            var newOrderStatus = new OrderStatus
            {
                Orderstate = orderStatus,
                Fkorderid = orderId,

            };

            _context.OrderStatuses.Add(newOrderStatus);
            _context.SaveChanges();

        }

        public void SaveOrderProduct(PayPalConfirmationVM model)
        {
            var products = _cookieRepo.GetProductsFromCookie();

            foreach (var product in products)
            {
                var newOrderProduct = new OrderProduct
                {
                    Qty = product.cartQty,
                    Unitprice = product.Price,
                    Fkorderid = GetOrderId(model),
                    Fkproductid = product.ID,
                };

                _context.OrderProducts.Add(newOrderProduct);
            }

            _context.SaveChanges();

        }

        public int GetOrderId(PayPalConfirmationVM model)
        {
            return _context.Orders
                        .Where(order => order.Pptransactionid == model.TransactionId)
                        .Select(order => order.Pkorderid)
                        .FirstOrDefault();
        }

        public decimal GetTotalAmount()
        {
            var products = _cookieRepo.GetProductsFromCookie() ?? new List<ProductVM>();

            decimal subtotal = 0;
            foreach (var product in products)
            {
                subtotal += product.Price * product.cartQty;

            }

            decimal gstRate = 0.05m;
            decimal pstRate = 0.07m;
            decimal gstAmount = subtotal * gstRate;
            decimal pstAmount = subtotal * pstRate;
            decimal totalTax = gstAmount + pstAmount;
            decimal total = subtotal + totalTax;

            return total;
        }

        public void UpdateProductStock()
        {
            var products = _cookieRepo.GetProductsFromCookie() ?? new List<ProductVM>();
            foreach (var prod in products)
            {
                var product = _context.Products
                                     .FirstOrDefault(p => p.Pkproductid == prod.ID);

                if (product != null)
                {
                    if (product.Qtyinstock >= prod.cartQty)
                    {
                        product.Qtyinstock -= prod.cartQty;
                    }
                    else
                    {

                        product.Qtyinstock = 0;
                    }
                }
            }
            _context.SaveChanges();
        }
    }


}

