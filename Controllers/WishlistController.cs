using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace peakmotion.Controllers
{
    public class WishlistController : Controller
    {
        private readonly WishlistRepo _wishlistRepo;
        private readonly PmuserRepo _pmuserRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CookieRepo _cookieRepo;

        public WishlistController(WishlistRepo wishlistRepo,
                                  PmuserRepo pmuserRepo,
                                  UserManager<IdentityUser> userManager,
                                  CookieRepo cookieRepo)
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

            // Check if product already in wishlist
            bool exists = _wishlistRepo.GetWishlistByUserId((int)userId)
                                       .Any(x => x.ProductId == productId);
            if (exists)
            {
                TempData["Message"] = "This product is already in your wishlist.";
                return RedirectToAction("Index", "Cart");
            }

            // Add to wishlist
            _wishlistRepo.AddToWishlist((int)userId, productId);

            // Also remove from cart if you want to do that automatically here?
            // Or do nothing: up to you

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

        // New action: Add item from wishlist to the cart, remove from wishlist, redirect to cart
        [HttpPost]
        public async Task<IActionResult> MoveToCart(int productId)
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

            // Remove from wishlist
            _wishlistRepo.RemoveFromWishlist((int)userId, productId);

            // Add to cart with quantity = 1
            AddToCartCookie(productId, 1);

            return RedirectToAction("Index", "Cart");
        }

        private void AddToCartCookie(int productId, int qty)
        {
            // Standard cookie-based cart approach
            var encodedCart = _cookieRepo.GetCookie("cart");
            var cartDict = new Dictionary<int, int>();

            if (!string.IsNullOrEmpty(encodedCart))
            {
                var decoded = WebUtility.UrlDecode(encodedCart);
                foreach (var segment in decoded.Split(","))
                {
                    var parts = segment.Split(":");
                    if (parts.Length == 2)
                    {
                        int pid = int.Parse(parts[0]);
                        int existingQty = int.Parse(parts[1]);
                        cartDict[pid] = existingQty;
                    }
                }
            }

            if (cartDict.ContainsKey(productId))
            {
                cartDict[productId] += qty;
            }
            else
            {
                cartDict[productId] = qty;
            }

            var updated = string.Join(",", cartDict.Select(x => $"{x.Key}:{x.Value}"));
            _cookieRepo.AddCookie("cart", WebUtility.UrlEncode(updated));
        }
    }
}
