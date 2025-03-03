using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using peakmotion.Models;

namespace peakmotion.ViewModels
{
        public class ProductVM
        {
                public int ID { get; set; }

                [Display(Name = "Name")]
                public string ProductName { get; set; }

                [Display(Name = "Description")]
                public string Description { get; set; } = String.Empty;

                [Display(Name = "Price")]
                [DisplayFormat(DataFormatString = "{0:C} CAD")]
                [Range(0, double.MaxValue)]
                public decimal Price { get; set; } = 0;

                [Display(Name = "Currency")]
                public string Currency { get; set; } = "CAD";

                [Display(Name = "Qty")]
                public int Quantity { get; set; } = 0;

                [Display(Name = "Is Featured")]
                public bool IsFeatured { get; set; } = false;

                [Display(Name = "Membership Product")]
                public bool IsMembershipProduct { get; set; } = false;

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

                public int? Pkdiscountid { get; set; }  // Nullable in case there's no discount applied


                public List<string> Colors { get; set; }  // List of available colors
                public List<string> Sizes { get; set; }   // List of available sizes

                public List<string> Types { get; set; }   // List of available sizes
                public List<string> Properties { get; set; }   // List of available sizes

                [BindNever, ValidateNever]
                public List<Category> ColorDropdown { get; set; } = new List<Category>();

                [BindNever, ValidateNever]
                public List<Category> SizeDropdown { get; set; } = new List<Category>();

                [BindNever, ValidateNever]
                public List<Category> TypeDropdown { get; set; } = new List<Category>();

                [BindNever, ValidateNever]
                public List<Category> PropertyDropdown { get; set; } = new List<Category>();

                [Display(Name = "Discount")]
                public int? SelectedDiscountId { get; set; }

                public string? SelectedDiscountDescription { get; set; } // Description of the selected discount

                [BindNever, ValidateNever]
                public List<Discount> DiscountDropdown { get; set; } = new List<Discount>();
                public string? photoName { get; set; } //alttage
                public string? ImagesToDelete { get; set; }


        }
}

