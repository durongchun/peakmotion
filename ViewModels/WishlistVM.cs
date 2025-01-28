using peakmotion.Models;
using System.Collections.Generic;

namespace peakmotion.ViewModels
{
    public class WishlistVM
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ProductImage? PrimaryImage { get; set; }
    }
}
