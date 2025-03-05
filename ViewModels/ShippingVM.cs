
using System.ComponentModel.DataAnnotations;
using peakmotion.Models;

namespace peakmotion.ViewModels
{
    public class ShippingVM
    {
        public int ID { get; set; }

        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [Display(Name = "Appt/Unit")]
        public string ApptUnit { get; set; }
        public string City { get; set; }

        public string Province { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        public Boolean IsSaveAddress { get; set; } = false;

    }
}