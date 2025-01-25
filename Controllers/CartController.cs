using System.Linq;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using System.Collections.Generic;
using System.Net; // For WebUtility (UrlEncode/UrlDecode)

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

        // Displays the cart contents
        public IActionResult Index()
        {
            var encodedCartString = _cookieRepo.GetCookie("cart");
            var cartItems = new List<CartItemVM>();

            if (!string.IsNullOrEmpty(encodedCartString))
            {
                var cartString = WebUtility.UrlDecode(encodedCartString);

                foreach (var productIDAndQty in cartString.Split(","))
                {
                    string[] arrIDAndQty = productIDAndQty.Split(":");
                    if (arrIDAndQty.Length == 2)
                    {
                        int productID = int.Parse(arrIDAndQty[0]);
                        int qty = int.Parse(arrIDAndQty[1]);

                        var product = _productRepo.GetProduct(productID);
                        if (product != null)
                        {
                            var cartItem = new CartItemVM
                            {
                                ProductID = product.ID,
                                ProductName = product.ProductName,
                                Price = product.Price,
                                CartQuantity = qty
                            };

                            cartItems.Add(cartItem);
                        }
                    }
                }
            }

            return View(cartItems);
        }

        // Adds a new product to the cart if it does not exist.
        // If the product exists, redirects to UpdateCartItem instead.
        public IActionResult AddCartItem(int productID, int qty = 1)
        {
            var product = _productRepo.GetProduct(productID);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index");
            }

            if (product.Quantity < qty)
            {
                TempData["Message"] = "Not enough inventory.";
                return RedirectToAction("Index");
            }

            var encodedCartString = _cookieRepo.GetCookie("cart");
            var cartItems = new List<CartItemVM>();
            bool productFoundInCart = false;
            int existingQty = 0;

            string cartString = WebUtility.UrlDecode(encodedCartString ?? "");

            if (!string.IsNullOrEmpty(cartString))
            {
                foreach (var productIDAndQty in cartString.Split(","))
                {
                    string[] arrIDAndQty = productIDAndQty.Split(":");
                    if (arrIDAndQty.Length == 2)
                    {
                        int currentProductID = int.Parse(arrIDAndQty[0]);
                        int currentQty = int.Parse(arrIDAndQty[1]);

                        if (currentProductID == productID)
                        {
                            productFoundInCart = true;
                            existingQty = currentQty;
                        }
                        else
                        {
                            var existingProduct = _productRepo.GetProduct(currentProductID);
                            if (existingProduct != null)
                            {
                                var cartItem = new CartItemVM
                                {
                                    ProductID = existingProduct.ID,
                                    ProductName = existingProduct.ProductName,
                                    Price = existingProduct.Price,
                                    CartQuantity = currentQty
                                };
                                cartItems.Add(cartItem);
                            }
                        }
                    }
                }
            }

            if (productFoundInCart)
            {
                int newQty = existingQty + qty;
                return RedirectToAction("UpdateCartItem", new { productID, newQty });
            }
            else
            {
                var newProductItem = new CartItemVM
                {
                    ProductID = product.ID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    CartQuantity = qty
                };
                cartItems.Add(newProductItem);
            }

            var updatedCartList = new List<string>();
            foreach (var item in cartItems)
            {
                updatedCartList.Add($"{item.ProductID}:{item.CartQuantity}");
            }
            var updatedCartString = string.Join(",", updatedCartList);

            // Encode and save to cookie
            var encodedValue = WebUtility.UrlEncode(updatedCartString);
            _cookieRepo.AddCookie("cart", encodedValue);

            return RedirectToAction("Index");
        }

        // Updates the quantity of an existing product in the cart
        public IActionResult UpdateCartItem(int productID, int newQty)
        {
            var product = _productRepo.GetProduct(productID);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index");
            }

            if (newQty < 1)
            {
                return RedirectToAction("DeleteCartItem", new { productID });
            }

            if (newQty > product.Quantity)
            {
                TempData["Message"] = "Not enough inventory.";
                newQty = product.Quantity;
            }

            var encodedCartString = _cookieRepo.GetCookie("cart");
            var cartItems = new List<CartItemVM>();
            bool updated = false;

            string cartString = WebUtility.UrlDecode(encodedCartString ?? "");

            if (!string.IsNullOrEmpty(cartString))
            {
                foreach (var productIDAndQty in cartString.Split(","))
                {
                    string[] arrIDAndQty = productIDAndQty.Split(":");
                    if (arrIDAndQty.Length == 2)
                    {
                        int currentProductID = int.Parse(arrIDAndQty[0]);
                        int currentQty = int.Parse(arrIDAndQty[1]);

                        var existingProduct = _productRepo.GetProduct(currentProductID);
                        if (existingProduct != null)
                        {
                            if (currentProductID == productID)
                            {
                                cartItems.Add(new CartItemVM
                                {
                                    ProductID = existingProduct.ID,
                                    ProductName = existingProduct.ProductName,
                                    Price = existingProduct.Price,
                                    CartQuantity = newQty
                                });
                                updated = true;
                            }
                            else
                            {
                                cartItems.Add(new CartItemVM
                                {
                                    ProductID = existingProduct.ID,
                                    ProductName = existingProduct.ProductName,
                                    Price = existingProduct.Price,
                                    CartQuantity = currentQty
                                });
                            }
                        }
                    }
                }
            }

            if (!updated)
            {
                TempData["Message"] = "Product was not in the cart to update.";
                return RedirectToAction("Index");
            }

            var updatedCartList = new List<string>();
            foreach (var item in cartItems)
            {
                updatedCartList.Add($"{item.ProductID}:{item.CartQuantity}");
            }
            var updatedCartString = string.Join(",", updatedCartList);

            var encodedValue = WebUtility.UrlEncode(updatedCartString);
            _cookieRepo.AddCookie("cart", encodedValue);

            return RedirectToAction("Index");
        }

        public IActionResult DeleteCartItem(int productID)
        {
            var encodedCartString = _cookieRepo.GetCookie("cart");
            var cartItems = new List<CartItemVM>();

            string cartString = WebUtility.UrlDecode(encodedCartString ?? "");

            if (!string.IsNullOrEmpty(cartString))
            {
                foreach (var productIDAndQty in cartString.Split(","))
                {
                    string[] arrIDAndQty = productIDAndQty.Split(":");
                    if (arrIDAndQty.Length == 2)
                    {
                        int currentProductID = int.Parse(arrIDAndQty[0]);
                        int currentQty = int.Parse(arrIDAndQty[1]);

                        if (currentProductID != productID)
                        {
                            var productObj = _productRepo.GetProduct(currentProductID);
                            if (productObj != null)
                            {
                                var cartItem = new CartItemVM
                                {
                                    ProductID = productObj.ID,
                                    ProductName = productObj.ProductName,
                                    Price = productObj.Price,
                                    CartQuantity = currentQty
                                };
                                cartItems.Add(cartItem);
                            }
                        }
                    }
                }
            }

            var updatedCartList = new List<string>();
            foreach (var item in cartItems)
            {
                updatedCartList.Add($"{item.ProductID}:{item.CartQuantity}");
            }
            var updatedCartString = string.Join(",", updatedCartList);

            var encodedValue = WebUtility.UrlEncode(updatedCartString);
            _cookieRepo.AddCookie("cart", encodedValue);

            return RedirectToAction("Index");
        }
    }
}
