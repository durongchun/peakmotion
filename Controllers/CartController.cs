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

        // Display the cart contents
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
                            cartItems.Add(new CartItemVM
                            {
                                ProductID = product.ID,
                                ProductName = product.ProductName,
                                Price = product.Price,
                                CartQuantity = qty,
                                MaxQuantity = product.Quantity // Set stock quantity
                            });
                        }
                    }
                }
            }

            // Pass TempData message to the view (if any)
            if (TempData["Message"] != null)
            {
                ViewBag.ErrorMessage = TempData["Message"];
            }

            return View(cartItems);
        }

        // Add a new product to the cart if it doesn't exist
        // If the product already exists, merge quantity by redirecting to Update
        [HttpPost]
        public IActionResult Add(int productID, int qty = 1)
        {
            var product = _productRepo.GetProduct(productID);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index");
            }

            // If the product is out of stock
            if (product.Quantity == 0)
            {
                TempData["Message"] = "This product is out of stock.";
                return RedirectToAction("Index");
            }

            // If requested quantity is more than available stock
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
                                cartItems.Add(new CartItemVM
                                {
                                    ProductID = existingProduct.ID,
                                    ProductName = existingProduct.ProductName,
                                    Price = existingProduct.Price,
                                    CartQuantity = currentQty,
                                    MaxQuantity = existingProduct.Quantity
                                });
                            }
                        }
                    }
                }
            }

            if (productFoundInCart)
            {
                int newQty = existingQty + qty;

                // If newQty exceeds the stock, handle it here
                if (newQty > product.Quantity)
                {
                    TempData["Message"] = "Not enough inventory.";
                    newQty = product.Quantity;
                }

                // Redirect to Update to merge the new quantity
                return RedirectToAction("Update", new { productID, newQty });
            }
            else
            {
                cartItems.Add(new CartItemVM
                {
                    ProductID = product.ID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    CartQuantity = qty,
                    MaxQuantity = product.Quantity
                });
            }

            // Update the cart string
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

        // Update the quantity of an existing product in the cart
        [HttpPost]
        public IActionResult Update(int productID, int newQty)
        {
            var product = _productRepo.GetProduct(productID);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index");
            }

            // If newQty is less than 1, treat it as a delete
            if (newQty < 1)
            {
                return RedirectToAction("Delete", new { productID });
            }

            // If newQty is more than the available stock
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
                                // Update the quantity here
                                cartItems.Add(new CartItemVM
                                {
                                    ProductID = existingProduct.ID,
                                    ProductName = existingProduct.ProductName,
                                    Price = existingProduct.Price,
                                    CartQuantity = newQty,
                                    MaxQuantity = existingProduct.Quantity
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
                                    CartQuantity = currentQty,
                                    MaxQuantity = existingProduct.Quantity
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

            // Rebuild the cart string
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

        [HttpPost]
        public IActionResult Delete(int productID)
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

                        // Add all items except the one to be deleted
                        if (currentProductID != productID)
                        {
                            var productObj = _productRepo.GetProduct(currentProductID);
                            if (productObj != null)
                            {
                                cartItems.Add(new CartItemVM
                                {
                                    ProductID = productObj.ID,
                                    ProductName = productObj.ProductName,
                                    Price = productObj.Price,
                                    CartQuantity = currentQty,
                                    MaxQuantity = productObj.Quantity
                                });
                            }
                        }
                    }
                }
            }

            // Rebuild the cart string after removal
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
