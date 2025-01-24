
using System.ComponentModel.DataAnnotations;
using peakmotion.Models;


namespace peakmotion.ViewModels
{
    public class ShopVM
    {
        public ShippingVM Shipping { get; set; }
        public ProductVM Cart { get; set; }
        public decimal TotalAmount { get; set; }
    }
}