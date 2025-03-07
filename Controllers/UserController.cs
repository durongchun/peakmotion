using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers
{
    public class UserController : Controller
    {
        private readonly PmuserRepo _pmuserRepo;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly CookieRepo _cookieRepo;

        public UserController(SignInManager<IdentityUser> signInManager, PmuserRepo pmuserRepo, UserManager<IdentityUser> userManager, ILogger<HomeController> logger, CookieRepo cookieRepo)
        {
            _pmuserRepo = pmuserRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _cookieRepo = cookieRepo;
        }

        public async Task<IActionResult> Logout()
        {
            _cookieRepo.RemoveCookie("cart");

            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser != null && identityUser.Email != null)
            {
                var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);
                if (userId != null)
                {
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("User logged out.");
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                else
                {
                    _logger.LogInformation("Could not find the PMuser by email");
                }
            }
            else
            {
                _logger.LogInformation("Could not find the Identity user.");
            }



            return Redirect("/Identity/Account/Login");
        }


    }
}
