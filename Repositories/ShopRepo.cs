using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace peakmotion.Repositories
{
    public class ShopRepo
    {
        private readonly PeakmotionContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public ShopRepo(PeakmotionContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IEnumerable<ShippingVM> GetShippingInfo()
        {
            IEnumerable<ShippingVM> shippingInfo = _context.Pmusers.Select(
                user => new ShippingVM
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
                }
            ).ToList();

            return shippingInfo;
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
            // Save confirmation to the database
            var newOrder = new Order
            {
                Pptransactionid = model.TransactionId,
                Orderdate = DateOnly.FromDateTime(DateTime.Now),
                Fkpmuserid = 1,

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
            var newOrderProduct = new OrderProduct
            {
                Qty = 1,
                Unitprice = 40.00m,
                Fkorderid = GetOrderId(model),
                Fkproductid = 1,

            };

            _context.OrderProducts.Add(newOrderProduct);
            _context.SaveChanges();

        }

        public int GetOrderId(PayPalConfirmationVM model)
        {
            return _context.Orders
                        .Where(order => order.Pptransactionid == model.TransactionId)
                        .Select(order => order.Pkorderid)
                        .FirstOrDefault();
        }

    }




}

