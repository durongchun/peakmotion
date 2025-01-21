using Microsoft.EntityFrameworkCore;
using peakmotion.Data;
using peakmotion.ViewModels;

namespace peakmotion.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ProductVM> GetAllProducts()
        {
            IEnumerable<ProductVM> products = _context.Products.Select(p => new ProductVM
            {
                ID = p.Pkproductid,
                Name = p.Name,
                Description = p.Description,
                Price = p.Regularprice,
                Quantity = p.Qtyinstock,
                IsFeatured = p.Isfeatured == 1 ? true : false,
                IsMembershipProduct = p.Ismembershipproduct == 1 ? true : false,
            });
            return products;
        }

    }
}