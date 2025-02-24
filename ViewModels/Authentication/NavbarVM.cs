using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace peakmotion.ViewModels
{
    public class NavbarVM
    {

        public string Controller { get; set; }

        public string Page { get; set; }

        public bool OnAdminPortal { get; set; }

        public string? RoleName { get; set; }


    }

}

