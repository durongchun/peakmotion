using System.ComponentModel.DataAnnotations;
using peakmotion.Models;

namespace peakmotion.ViewModels
{
    public class UserVM
    {
        [Key]
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName}{(((FirstName != null) && (LastName != null)) ? ' ' : string.Empty)}{LastName}";

    }
}
