using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;

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
            IEnumerable<ProductVM> products = _context.Products.Select(p => new ProductVM
            {
                ID = p.Pkproductid,
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Regularprice,
                // Currency = p.Currency,
                // Image = p.Image
            });

            return products;
        }
    }
}

