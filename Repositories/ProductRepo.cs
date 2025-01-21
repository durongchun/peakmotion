using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Repositories
{
    public class ProductRepo
    {
        private readonly PeakmotionContext _context;

        public ProductRepo(PeakmotionContext context)
        {
            _context = context;
        }

        // Get specific product in the database.
        public ProductVM? GetProduct(int id)
        {
            ProductVM? product = _context.Products.Select(p => new ProductVM
            {
                ID = p.Pkproductid,
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Regularprice,
                // Currency = p.Currency,
                // Image = p.Image
            }).FirstOrDefault(p => p.ID == id);

            return product;
        }

        // Get all products in the database.
        public IEnumerable<ProductVM> GetAllProducts()
        {
            IEnumerable<ProductVM> products = _context.Products
            .Include(p => p.Fkdiscount)
            .Include(p => p.ProductCategories)
            .Include(p => p.ProductImages)
            .Select(p => new ProductVM
            {
                ID = p.Pkproductid,
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Regularprice,
                Quantity = p.Qtyinstock,
                IsFeatured = p.Isfeatured == 1 ? true : false,
                IsMembershipProduct = p.Ismembershipproduct == 1 ? true : false,
                Discount = p.Fkdiscount,
                Categories = p.ProductCategories
                            .Where(pc => pc.Fkproductid == p.Pkproductid)
                            .Select(x => x.Fkcategory)
                            .ToList(),
                Images = p.ProductImages,
            });


            // IEnumerable<ProductVM> products = from p in _context.Products
            //                                   join pc in _context.ProductCategories on p.Pkproductid equals pc.Fkproductid
            //                                   join pi in _context.ProductImages on p.Pkproductid equals pi.Fkproductid
            //                                   //   join d in _context.Discounts on p.Fkdiscountid equals d.Pkdiscountid 
            //                                   //   where p.Qtyinstock > 0
            //                                   select new ProductVM
            //                                   {
            //                                       ID = p.Pkproductid,
            //                                       ProductName = p.Name,
            //                                       Description = p.Description,
            //                                       Price = p.Regularprice,
            //                                       Quantity = p.Qtyinstock,
            //                                       IsFeatured = p.Isfeatured == 1 ? true : false,
            //                                       IsMembershipProduct = p.Ismembershipproduct == 1 ? true : false,
            //                                       //   Discount = p.Fkdiscount == null ? null : p.Fkdiscount,
            //                                       Categories = p.ProductCategories
            //                                                       .Where(pc => pc.Fkproductid == p.Pkproductid)
            //                                                       .Select(x => x.Fkcategory)
            //                                                       .ToList(),
            //                                       //   Images = p.ProductImages,
            //                                   };
            return products;
        }
    }
}

