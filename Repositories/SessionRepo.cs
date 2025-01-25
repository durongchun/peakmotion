using Microsoft.AspNetCore.Http;
using peakmotion.Data;
using peakmotion.Models;
using Newtonsoft.Json;

namespace peakmotion.Repositories
{
    public class SessionRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _httpContext;

        public SessionRepo(IHttpContextAccessor httpContextAccessor)
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

                // Create an object to store product data
                var productData = new
                {
                    ProductId = productId,
                    ProductName = productName,
                    ProductPrice = productPrice,
                    Quantity = quantity
                };

                // Serialize product data to a JSON string
                var serializedData = JsonConvert.SerializeObject(productData);

                // Save serialized data to a cookie
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
                    // Deserialize the cookie data
                    var product = JsonConvert.DeserializeObject<dynamic>(productDataCookie);

                    // Add data to the list
                    productData.Add("Product ID: " + ((product.ProductId?.ToString() ?? "No ID").Replace("[", "").Replace("]", "").Replace("\"", "").Trim()));
                    productData.Add("Product Name: " + ((product.ProductName?.ToString() ?? "No Name").Replace("[", "").Replace("]", "").Replace("\"", "").Trim()));
                    productData.Add("Regular Price: " + ((product.ProductPrice?.ToString() ?? "No Price").Replace("[", "").Replace("]", "").Replace("\"", "").Trim()));
                    productData.Add("Qty: " + ((product.Quantity?.ToString() ?? "No Quantity").Replace("[", "").Replace("]", "").Replace("\"", "").Trim()));


                }
            }

            return productData;
        }

    }

}
