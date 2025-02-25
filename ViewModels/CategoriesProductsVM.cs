using peakmotion.Models;

namespace peakmotion.ViewModels
{
    // For the Product List Page
    public class CategoriesProductsVM
    {
        // Product list
        public required IEnumerable<ProductVM> Products { get; set; }

        // Category list and values
        public Dictionary<string, List<Category>> Filters { get; set; } = new Dictionary<string, List<Category>>();

    }
}

