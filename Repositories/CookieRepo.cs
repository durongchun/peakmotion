using Microsoft.AspNetCore.Http;
using peakmotion.Data;
using peakmotion.Models;
using Newtonsoft.Json;

namespace peakmotion.Repositories
{
    public class CookieRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _httpContext;

        public CookieRepo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpContext = _httpContextAccessor.HttpContext;  // Store the HttpContext once
        }

        public void SetProductDataToSession()
        {
            if (_httpContext?.Request.HasFormContentType == true)
            {
                // Retrieve form values
                var productId = _httpContext.Request.Form["ProductId"];
                var productName = _httpContext.Request.Form["ProductName"];
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

        public void RemoveCookie(string key)
        {
            var existingData = _httpContext.Request.Cookies[key];
            if (!string.IsNullOrEmpty(existingData))
            {
                // Delete the cookie with the specified key
                _httpContext.Response.Cookies.Delete(key);
            }
        }

    }

}