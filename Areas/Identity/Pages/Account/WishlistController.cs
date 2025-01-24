using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers
{
    public class WishlistController : Controller
    {
        private readonly WishlistRepo _wishlistRepo;

        public WishlistController(WishlistRepo wishlistRepo)
        {
            _wishlistRepo = wishlistRepo;
        }

        public IActionResult Index()
        {
            // Replace with logic for retrieving the current user's ID.
            int userId = 1;

            IEnumerable<WishlistVM> wishlistItems = _wishlistRepo.GetWishlistByUserId(userId);
            return View(wishlistItems);
        }

        [HttpPost]
        public IActionResult Add(int productId)
        {
            int userId = 1;
            _wishlistRepo.AddToWishlist(userId, productId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            int userId = 1;
            _wishlistRepo.RemoveFromWishlist(userId, productId);
            return RedirectToAction(nameof(Index));
        }
    }
}
