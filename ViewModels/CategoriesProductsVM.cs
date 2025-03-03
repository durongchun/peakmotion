using Microsoft.AspNetCore.Mvc.Rendering;
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

        public int? FilterId { get; set; }

        // Sort Options
        public SelectList SortOptions { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        // default is featured
        public string SortByChoice { get; set; } = "Featured";

    }
}

