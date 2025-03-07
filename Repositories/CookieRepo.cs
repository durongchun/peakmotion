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
