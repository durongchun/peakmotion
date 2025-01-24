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

                // Save values to session
                _httpContext.Session.SetString("ProductId", productId);
                _httpContext.Session.SetString("ProductName", productName);
                _httpContext.Session.SetString("ProductPrice", productPrice);
                _httpContext.Session.SetString("Quantity", quantity);
            }
        }

        public List<string> GetUserChosenProductInfo()
        {
            var productData = new List<string>();

            if (_httpContext != null)
            {
                // Retrieve product data from session
                var productId = _httpContext.Session.GetString("ProductId");
                var productName = _httpContext.Session.GetString("ProductName");
                var productPrice = _httpContext.Session.GetString("ProductPrice");
                var quantity = _httpContext.Session.GetString("Quantity");

                if (!string.IsNullOrEmpty(productName))
                {
                    // Add data to the list
                    productData.Add("Product ID: " + productId ?? "No ID");
                    productData.Add("Product Name: " + productName ?? "No Name");
                    productData.Add("Regular Price: " + productPrice ?? "No Price");
                    productData.Add("Qty: " + quantity ?? "No Quantity");
                }
            }

            return productData;
        }
    }

}
