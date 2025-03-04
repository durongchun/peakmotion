using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peakmotion.Controllers
{
    public class WishlistController : Controller
    {
        private readonly WishlistRepo _wishlistRepo;
        private readonly PmuserRepo _pmuserRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CookieRepo _cookieRepo;

        public WishlistController(
            WishlistRepo wishlistRepo,
            PmuserRepo pmuserRepo,
            UserManager<IdentityUser> userManager,
            CookieRepo cookieRepo
        )
        {
            _wishlistRepo = wishlistRepo;
            _pmuserRepo = pmuserRepo;
            _userManager = userManager;
            _cookieRepo = cookieRepo;
        }

        public async Task<IActionResult> Index()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }
            else
            {
                var items = _wishlistRepo.GetWishlistByUserId((int)userId);
                return View(items);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var alreadyExists = _wishlistRepo.GetWishlistByUserId((int)userId)
                .Any(x => x.ProductId == productId);
            if (alreadyExists)
            {
                TempData["Message"] = "This product is already in your wishlist.";
                return RedirectToAction("Index", "Cart");
            }

            _wishlistRepo.AddToWishlist((int)userId, productId);
            RemoveFromCart(productId);
            TempData["Message"] = "Product saved for later.";
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var userId = _pmuserRepo.GetUserIdByUserEmail(identityUser.Email);
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            _wishlistRepo.RemoveFromWishlist((int)userId, productId);
            return RedirectToAction(nameof(Index));
        }

        private void RemoveFromCart(int productId)
        {
            var encoded = _cookieRepo.GetCookie("cart");
            var dict = new Dictionary<int, int>();
            if (!string.IsNullOrEmpty(encoded))
            {
                var decoded = WebUtility.UrlDecode(encoded);
                foreach (var segment in decoded.Split(","))
                {
                    var parts = segment.Split(":");
                    if (parts.Length == 2)
                    {
                        int pid = int.Parse(parts[0]);
                        int qty = int.Parse(parts[1]);
                        if (pid != productId)
                        {
                            dict[pid] = qty;
                        }
                    }
                }
            }
            var updated = string.Join(",", dict.Select(x => $"{x.Key}:{x.Value}"));
            _cookieRepo.AddCookie("cart", WebUtility.UrlEncode(updated));
        }
    }
}
