using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using peakmotion.Models;
using peakmotion.Repositories;

namespace peakmotion.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly PmuserRepo _pmuserRepo;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            PmuserRepo pmuserRepo
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _pmuserRepo = pmuserRepo;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public bool ShowDashboard { get; set; }

        [BindProperty]
        public bool IsEditing { get; set; }

        [BindProperty]
        public EditInputModel Input { get; set; }

        public string DisplayEmail { get; set; } = "";
        public string DisplayFirstName { get; set; } = "";
        public string DisplayLastName { get; set; } = "";
        public string DisplayPhone { get; set; } = "";
        public string DisplayAddress { get; set; } = "";
        public string DisplayCity { get; set; } = "";
        public string DisplayProvince { get; set; } = "";
        public string DisplayPostalCode { get; set; } = "";
        public string DisplayCountry { get; set; } = "";

        public class EditInputModel
        {
            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }
            [Required]
            public string Phone { get; set; }
            [Required]
            public string Address { get; set; }
            [Required]
            public string City { get; set; }
            [Required]
            public string Province { get; set; }
            [Required]
            public string PostalCode { get; set; }
            [Required]
            public string Country { get; set; }

            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool? dashboard)
        {
            ShowDashboard = (dashboard == true);
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            bool? dashboard,
            string edit,
            string save,
            string cancel
        )
        {
            ModelState.Remove("edit");
            ModelState.Remove("cancel");
            ShowDashboard = (dashboard == true);

            if (!string.IsNullOrEmpty(edit))
            {
                IsEditing = true;
                await LoadAsync();
                return Page();
            }

            if (!string.IsNullOrEmpty(cancel))
            {
                IsEditing = false;
                await LoadAsync();
                return Page();
            }

            if (!string.IsNullOrEmpty(save))
            {
                if (string.IsNullOrWhiteSpace(Input.NewPassword))
                {
                    ModelState.Remove("Input.CurrentPassword");
                    ModelState.Remove("Input.NewPassword");
                    ModelState.Remove("Input.ConfirmPassword");
                }

                if (!ModelState.IsValid)
                {
                    StatusMessage = "Error: Required fields are missing or invalid.";
                    IsEditing = true;
                    await LoadAsync();
                    return Page();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    StatusMessage = "Error: User not found.";
                    return Page();
                }

                if (!string.IsNullOrWhiteSpace(Input.NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
                    {
                        ModelState.AddModelError(string.Empty, "Current password is required to change the password.");
                        StatusMessage = "Error: Current password required.";
                        IsEditing = true;
                        await LoadAsync();
                        return Page();
                    }
                    var changePasswordResult = await _userManager.ChangePasswordAsync(
                        user,
                        Input.CurrentPassword,
                        Input.NewPassword
                    );
                    if (!changePasswordResult.Succeeded)
                    {
                        foreach (var error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        StatusMessage = "Error changing password.";
                        IsEditing = true;
                        await LoadAsync();
                        return Page();
                    }
                }

                var pmUser = _pmuserRepo.GetPmuserByEmail(user.Email);
                if (pmUser == null)
                {
                    StatusMessage = "Error: PMUser record not found.";
                    IsEditing = true;
                    await LoadAsync();
                    return Page();
                }

                pmUser.Firstname = Input.FirstName;
                pmUser.Lastname = Input.LastName;
                pmUser.Phone = Input.Phone;
                pmUser.Address = Input.Address;
                pmUser.City = Input.City;
                pmUser.Province = Input.Province;
                pmUser.Postalcode = Input.PostalCode;
                pmUser.Country = Input.Country;

                try
                {
                    _pmuserRepo.UpdatePmuser(pmUser);
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error updating profile: " + ex.Message);
                    StatusMessage = "Error: Profile update failed.";
                    IsEditing = true;
                    await LoadAsync();
                    return Page();
                }

                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Profile updated successfully.";
                IsEditing = false;
                await LoadAsync();
                return Page();
            }

            await LoadAsync();
            return Page();
        }

        private async Task LoadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            DisplayEmail = user.Email ?? "";
            var pmUser = _pmuserRepo.GetPmuserByEmail(user.Email);
            if (pmUser != null)
            {
                DisplayFirstName = pmUser.Firstname ?? "";
                DisplayLastName = pmUser.Lastname ?? "";
                DisplayPhone = pmUser.Phone ?? "";
                DisplayAddress = pmUser.Address ?? "";
                DisplayCity = pmUser.City ?? "";
                DisplayProvince = pmUser.Province ?? "";
                DisplayPostalCode = pmUser.Postalcode ?? "";
                DisplayCountry = pmUser.Country ?? "";
            }

            Input = new EditInputModel
            {
                FirstName = DisplayFirstName,
                LastName = DisplayLastName,
                Phone = DisplayPhone,
                Address = DisplayAddress,
                City = DisplayCity,
                Province = DisplayProvince,
                PostalCode = DisplayPostalCode,
                Country = DisplayCountry
            };
        }
    }
}
