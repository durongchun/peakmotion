using System.ComponentModel.DataAnnotations;
using peakmotion.Models;

namespace peakmotion.ViewModels
{
    public class CartItemVM
    {
        public int ProductID { get; set; }
        [Display(Name = "Name")]
        public string ProductName { get; set; }
        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C} CAD")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } = 0;
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "CAD";
        [Display(Name = "CartQty")]
        public int CartQuantity { get; set; } = 0;
        public Discount? Discount { get; set; }
        [Display(Name = "Discount")]
        public string? DiscountLabel =>
            Discount != null && Discount.Description == "discount"
                ? $"${Discount.Amount} OFF"
                : Discount != null && Discount.Description == "free shipping"
                ? "Free shipping"
                : null;
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        [Display(Name = "Images")]
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        [Display(Name = "Main Image")]
        public ProductImage? PrimaryImage { get; set; }
    }
}
