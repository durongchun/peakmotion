
using System.ComponentModel.DataAnnotations;
using peakmotion.Models;

namespace peakmotion.ViewModels
{
    public class ProductVM
    {
        public int ID { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; } = String.Empty;

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "${0:C} CAD")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } = 0;

        [Display(Name = "Currency")]
        public string Currency { get; set; } = "CAD";

        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 0;

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Is Membership Product")]
        public bool IsMembershipProduct { get; set; } = false;

        public Discount? Discount { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        [Display(Name = "Images")]
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    }
}

