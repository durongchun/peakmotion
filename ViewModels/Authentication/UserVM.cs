using System.ComponentModel.DataAnnotations;

namespace peakmotion.ViewModels
{
    public class UserVM
    {
        [Required]
        public string? Email { get; set; }
    }
}
