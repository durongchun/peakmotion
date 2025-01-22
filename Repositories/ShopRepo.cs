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

        public void SaveShippingInfo(String firstname, String lastname, String phone, String address, String city,
                                                        String province, String postalcode, String country, String email)
        {
            var userShippingInfo = new Pmuser
            {
                Firstname = firstname,
                Lastname = lastname,
                Phone = phone,
                Address = address,
                City = city,
                Province = province,
                Postalcode = postalcode,
                Country = country,
                Email = email,

            };

            _context.Pmusers.Add(userShippingInfo);
            _context.SaveChanges();
        }

        // Get specific product in the database.
        // public ProductVM? GetProduct(int id)s
        // {
        //     ProductVM? product = _context.Products.Select(p => new ProductVM
        //     {
        //         ID = p.Pkproductid,
        //         ProductName = p.Name,
        //         Description = p.Description,
        //         Price = p.Regularprice,
        //         // Currency = p.Currency,
        //         // Image = p.Image
        //     }).FirstOrDefault(p => p.ID == id);

        //     return product;
        // }

        // // Get all products in the database.
        // public IEnumerable<ProductVM> GetAllProducts()
        // {
        //     IEnumerable<ProductVM> products = from p in _context.Products
        //                                       join pc in _context.ProductCategories on p.Pkproductid equals pc.Fkproductid into pCatGroup
        //                                       from pc in pCatGroup.DefaultIfEmpty()
        //                                       join pi in _context.ProductImages on p.Pkproductid equals pi.Fkproductid into pImageGroup
        //                                       from pi in pImageGroup.DefaultIfEmpty()
        //                                       join d in _context.Discounts on p.Fkdiscountid equals d.Pkdiscountid into productGroup
        //                                       from d in productGroup.DefaultIfEmpty()
        //                                       select new ProductVM
        //                                       {
        //                                           ID = p.Pkproductid,
        //                                           ProductName = p.Name,
        //                                           Description = p.Description,
        //                                           Price = p.Regularprice,
        //                                           Quantity = p.Qtyinstock,
        //                                           IsFeatured = p.Isfeatured == 1 ? true : false,
        //                                           IsMembershipProduct = p.Ismembershipproduct == 1 ? true : false,
        //                                           Discount = p.Fkdiscount,
        //                                           Categories = p.ProductCategories
        //                                               .Where(pc => pc.Fkproductid == p.Pkproductid)
        //                                               .Select(x => x.Fkcategory)
        //                                               .ToList(),
        //                                       };
        //     return products;
        // }
    }
}

