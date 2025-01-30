using System.ComponentModel.DataAnnotations;

namespace peakmotion.ViewModels
{
    public class RoleVM
    {
        [Required]
        [Display(Name = "Role Name")]
        public string? RoleName { get; set; }
    }
}

