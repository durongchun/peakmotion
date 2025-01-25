using System.Linq;
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

        public IActionResult Index()
        {
            var cartString = _cookieRepo.GetCookie("cart");
            var cartItems = new List<CartItemVM>();

            if (!string.IsNullOrEmpty(cartString))
            {
                var productIds = cartString.Split(',')
                    .Select(x => int.Parse(x))
                    .Distinct()
                    .ToList();

                foreach (var id in productIds)
                {
                    var product = _productRepo.GetProduct(id);
                    if (product != null)
                    {
                        cartItems.Add(new CartItemVM
                        {
                            ProductID = product.ID,
                            ProductName = product.ProductName,
                            Price = product.Price,
                            CartQuantity = 1,
                            Discount = product.Discount,
                            Categories = product.Categories,
                            Images = product.Images,
                            PrimaryImage = product.PrimaryImage
                        });
                    }
                }
            }

            return View(cartItems);
        }

        public IActionResult AddCartItem(int productId)
        {
            var cartString = _cookieRepo.GetCookie("cart");
            if (string.IsNullOrEmpty(cartString))
            {
                _cookieRepo.AddCookie("cart", productId.ToString());
            }
            else
            {
                var productIds = cartString.Split(',')
                    .Select(x => int.Parse(x))
                    .ToList();

                if (!productIds.Contains(productId))
                {
                    productIds.Add(productId);
                }

                var updatedString = string.Join(",", productIds);
                _cookieRepo.AddCookie("cart", updatedString);
            }

            return RedirectToAction("Index");
        }

        public IActionResult DeleteCartItem(int productId)
        {
            var cartString = _cookieRepo.GetCookie("cart");
            if (!string.IsNullOrEmpty(cartString))
            {
                var productIds = cartString.Split(',')
                    .Select(x => int.Parse(x))
                    .ToList();

                if (productIds.Contains(productId))
                {
                    productIds.Remove(productId);
                }

                var updatedString = string.Join(",", productIds);
                _cookieRepo.AddCookie("cart", updatedString);
            }

            return RedirectToAction("Index");
        }
    }
}
