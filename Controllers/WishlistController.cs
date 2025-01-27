using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers
{
    public class WishlistController : Controller
    {
        private readonly WishlistRepo _wishlistRepo;
        private readonly PmuserRepo _pmuserRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public WishlistController(WishlistRepo wishlistRepo, PmuserRepo pmuserRepo, UserManager<IdentityUser> userManager)
        {
            _wishlistRepo = wishlistRepo;
            _pmuserRepo = pmuserRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else{
                IEnumerable<WishlistVM> wishlistItems = _wishlistRepo.GetWishlistByUserId(userId);
                return View(wishlistItems);
            }


        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                _wishlistRepo.AddToWishlist(userId, productId);
                return RedirectToAction(nameof(Index));
            }

        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                _wishlistRepo.RemoveFromWishlist(userId, productId);
                return RedirectToAction(nameof(Index));
            }

        }
    }
}
