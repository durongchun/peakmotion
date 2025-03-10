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

        public int GetCartqtyFromCookie()
        {
            var encodedCartString = GetCookie("cart");
            var qty = 0;

            if (!string.IsNullOrEmpty(encodedCartString))
            {
                var decoded = WebUtility.UrlDecode(encodedCartString);
                foreach (var segment in decoded.Split(","))
                {
                    var parts = segment.Split(":");
                    if (parts.Length == 2)
                    {
                        qty += int.Parse(parts[1]);
                    }
                }
            }

            return qty;
        }

        public void AddPropertyToCookie(string selectedColor, string selectedSize, int productid)
        {
            int expireDays = 7;
            var properties = new List<string>();

            // Retrieve the existing cookie
            var existingCookie = GetCookie("Property");
            if (existingCookie != null)
            {
                if (existingCookie.Contains(productid.ToString()))
                {
                    existingCookie = existingCookie.Replace(productid.ToString(), "");
                }
                // Deserialize the existing cookie value into a list
                properties = JsonConvert.DeserializeObject<List<string>>(existingCookie);
            }

            // Create the new property
            var property = productid + ":" + selectedColor + ":" + selectedSize;

            // Add the new property to the list (if it doesn't already exist)
            if (!properties.Contains(property))
            {
                properties.Add(property);
            }

            // Serialize the updated list
            var serializedProperties = JsonConvert.SerializeObject(properties);

            // Add or update the cookie with the new serialized list
            AddCookie("Property", serializedProperties, expireDays);
        }



        public List<string> GetPropertyFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return new List<string>();

            var cookieValue = context.Request.Cookies["Property"];
            if (string.IsNullOrEmpty(cookieValue)) return new List<string>();

            try
            {
                // Deserialize as List<string>
                var properties = JsonConvert.DeserializeObject<List<string>>(cookieValue);
                return properties ?? new List<string>();
            }
            catch (Exception)
            {
                // If deserialization fails, return an empty list
                return new List<string>();
            }
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

        public void AddSaveStatusToCookie(ShippingVM model, int expireDays = 7)
        {
            var serializedShippingInfo = JsonConvert.SerializeObject(model.IsSaveAddress);
            AddCookie("Status", serializedShippingInfo, expireDays);
        }

        public bool GetSaveStatusFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var cookieValue = context.Request.Cookies["Status"];
            if (string.IsNullOrEmpty(cookieValue)) return false;

            var status = JsonConvert.DeserializeObject<bool>(cookieValue);
            return status;




        }


    }

}
