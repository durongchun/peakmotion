using System.ComponentModel.DataAnnotations;
using peakmotion.Models;

namespace peakmotion.ViewModels
{
    public class ProductVM
    {
        public int ID { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "${0:C} CAD")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Is Membership Product")]
        public bool IsMembershipProduct { get; set; }

        [Display(Name = "Image")]
        public ICollection<ProductImage>? ProductImages { get; set; }

        public decimal? Discount { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    }
}
