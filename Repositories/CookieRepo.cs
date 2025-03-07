using Microsoft.AspNetCore.Http;
using peakmotion.Data;
using peakmotion.Models;
using Newtonsoft.Json;
using peakmotion.ViewModels;
using System.Net;

namespace peakmotion.Repositories
{
    public class CookieRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _httpContext;
        private readonly ProductRepo _productPepo;

        public CookieRepo(IHttpContextAccessor httpContextAccessor, ProductRepo productPepo)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpContext = _httpContextAccessor.HttpContext;  // Store the HttpContext once
            _productPepo = productPepo;
        }

        public void SetProductDataToCookie(ProductDetailVM productDetailViewModel)
        {
            if (_httpContext?.Request.HasFormContentType == true)
            {
                // Retrieve form values
                var productId = productDetailViewModel.Pkproductid;
                var productName = productDetailViewModel.Productname;
                var productPrice = _httpContext.Request.Form["ProductPrice"];
                var quantity = _httpContext.Request.Form["Quantity"];

                // Create a new product object
                var newProduct = new
                {
                    ProductId = productId,
                    ProductName = productName,
                    ProductPrice = productPrice,
                    Quantity = quantity
                };

                // Retrieve existing product data from cookies
                var existingData = _httpContext.Request.Cookies["ProductData"];
                var productList = new List<dynamic>();

                if (!string.IsNullOrEmpty(existingData))
                {
                    try
                    {
                        // Deserialize existing data into a list
                        productList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
                    }
                    catch (JsonSerializationException ex)
                    {
                        // Handle invalid JSON format (e.g., if a single object was mistakenly saved)
                        _httpContext.Response.Cookies.Delete("ProductData"); // Clear the corrupted data
                    }
                }

                // Add the new product to the list
                productList.Add(newProduct);

                // Serialize the updated product list back to JSON
                var serializedData = JsonConvert.SerializeObject(productList);

                // Save the updated product list to the cookie
                _httpContext.Response.Cookies.Append("ProductData", serializedData, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddHours(1), // Set expiration as needed
                    HttpOnly = true, // Secure flag for better security
                    Secure = _httpContext.Request.IsHttps // Set Secure flag for HTTPS requests
                });
            }
        }
        public List<string> GetUserChosenProductInfoFromCookies()
        {
            var productData = new List<string>();

            if (_httpContext != null)
            {
                // Retrieve product data from cookies
                var productDataCookie = _httpContext.Request.Cookies["ProductData"];

                if (!string.IsNullOrEmpty(productDataCookie))
                {
                    try
                    {
                        // Deserialize the cookie data into a list of objects
                        var products = JsonConvert.DeserializeObject<List<dynamic>>(productDataCookie);

                        // Iterate through each product in the list
                        foreach (var product in products)
                        {
                            var productId = product.ProductId?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() ?? "No ID";
                            var productName = product.ProductName?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() ?? "No Name";
                            var productPrice = product.ProductPrice?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() ?? "No Price";
                            var quantity = product.Quantity?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() ?? "No Quantity";

                            // Format the product data and add it to the list
                            productData.Add($"Product ID: {productId}, Product Name: {productName}, Regular Price: {productPrice}, Qty: {quantity}");
                        }
                    }
                    catch (JsonSerializationException ex)
                    {
                        // Handle invalid JSON
                        _httpContext.Response.Cookies.Delete("ProductData");
                        throw new Exception("Failed to parse product data from cookies.", ex);
                    }
                }
            }

            return productData;
        }

        public void AddCookie(string key, string value, int expireDays = 7)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(expireDays),
                Path = "/"
                //HttpOnly = true, // JS can't access the cookie
                //Secure = true // only send cookie over HTTPS
            };
            context.Response.Cookies.Append(key, value, options);
        }

        public string GetCookie(string key)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;
            return context.Request.Cookies[key];
        }

        public void RemoveCookie(string key)
        {
            var existingData = _httpContext.Request.Cookies[key];
            if (!string.IsNullOrEmpty(existingData))
            {
                // Delete the cookie with the specified key
                _httpContext.Response.Cookies.Delete(key);
            }
        }

        public List<ProductVM> GetProductsFromCookie()
        {
            var encodedCartString = GetCookie("cart");
            var products = new List<ProductVM>();

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
                        var product = _productPepo.GetProductById(productId, qty);
                        if (product != null)
                        {
                            products.Add(product);

                        }
                    }
                }
            }

            return products ?? new List<ProductVM>();

        }

        public void AddShippingVMToCookie(ShippingVM shippingInfoList, int expireDays = 7)
        {
            var serializedShippingInfo = JsonConvert.SerializeObject(shippingInfoList);
            AddCookie("ShippingData", serializedShippingInfo, expireDays);
        }

        public ShippingVM GetShippingVMFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var cookieValue = context.Request.Cookies["ShippingData"];
            if (string.IsNullOrEmpty(cookieValue)) return null;

            var shippingInfo = JsonConvert.DeserializeObject<ShippingVM>(cookieValue);
            return shippingInfo;
        }

    }

}
