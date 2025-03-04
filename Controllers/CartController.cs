using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers
{
    public class CartController : Controller
    {
        private readonly CookieRepo _cookieRepo;
        private readonly ProductRepo _productRepo;

        public CartController(CookieRepo cookieRepo, ProductRepo productRepo)
        {
            _cookieRepo = cookieRepo;
            _productRepo = productRepo;
        }

        // Display the cart using ProductVM + a dictionary for quantities in ViewBag
        public IActionResult Index()
        {
            var encodedCartString = _cookieRepo.GetCookie("cart");
            var products = new List<ProductVM>();
            var quantities = new Dictionary<int, int>();

            if (!string.IsNullOrEmpty(encodedCartString))
            {
                var decoded = WebUtility.UrlDecode(encodedCartString);
                foreach (var segment in decoded.Split(","))
                {
                    var parts = segment.Split(":");
                    if (parts.Length == 2)
                    {
                        int productId = int.Parse(parts[0]);
                        int qty = int.Parse(parts[1]);
                        var product = _productRepo.GetProduct(productId);
                        if (product != null)
                        {
                            products.Add(product);
                            quantities[productId] = qty;
                        }
                    }
                }
            }

            if (TempData["Message"] != null)
            {
                ViewBag.ErrorMessage = TempData["Message"];
            }

            // We pass the dictionary in ViewBag so the view can show each product's quantity
            ViewBag.CartQuantities = quantities;
            return View(products);
        }

        [HttpPost]
        public IActionResult Add(int productId, int qty = 1)
        {
            var product = _productRepo.GetProduct(productId);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index");
            }

            if (product.Quantity == 0)
            {
                TempData["Message"] = "This product is out of stock.";
                return RedirectToAction("Index");
            }

            if (qty > product.Quantity)
            {
                TempData["Message"] = "Not enough inventory.";
                return RedirectToAction("Index");
            }

            // Decode existing cart
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

            // Merge quantity
            if (cartDict.ContainsKey(productId))
            {
                int newQty = cartDict[productId] + qty;
                if (newQty > product.Quantity)
                {
                    TempData["Message"] = "Not enough inventory.";
                    newQty = product.Quantity;
                }
                cartDict[productId] = newQty;
            }
            else
            {
                cartDict[productId] = qty;
            }

            // Encode back to cookie
            var updated = string.Join(",", cartDict.Select(x => $"{x.Key}:{x.Value}"));
            _cookieRepo.AddCookie("cart", WebUtility.UrlEncode(updated));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int productId, int newQty)
        {
            var product = _productRepo.GetProduct(productId);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index");
            }

            if (newQty < 1)
            {
                return RedirectToAction("Remove", new { productId });
            }

            if (newQty > product.Quantity)
            {
                TempData["Message"] = "Not enough inventory.";
                newQty = product.Quantity;
            }

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
                cartDict[productId] = newQty;
            }
            else
            {
                // If product not in cart, just add it
                cartDict[productId] = newQty;
            }

            var updated = string.Join(",", cartDict.Select(x => $"{x.Key}:{x.Value}"));
            _cookieRepo.AddCookie("cart", WebUtility.UrlEncode(updated));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
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
                        if (pid != productId)
                        {
                            cartDict[pid] = existingQty;
                        }
                    }
                }
            }

            var updated = string.Join(",", cartDict.Select(x => $"{x.Key}:{x.Value}"));
            _cookieRepo.AddCookie("cart", WebUtility.UrlEncode(updated));
            return RedirectToAction("Index");
        }
    }
}
