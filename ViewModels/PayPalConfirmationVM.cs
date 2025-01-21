using System.ComponentModel.DataAnnotations;

namespace peakmotion.ViewModels
{
    public class PayPalConfirmationVM
    {

        [Display(Name = "Payment ID")]
        public string TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        [Display(Name = "Name")]
        public string PayerName { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; }

        public string Email { get; set; }

        public string MOP { get; set; }
    }
}