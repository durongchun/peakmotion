using Microsoft.AspNetCore.Http;
using System;

namespace peakmotion.Repositories
{
    public class CookieRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieRepo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddCookie(string key, string value, int expireDays = 7)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(expireDays),
                HttpOnly = true, // JS can't access the cookie
                Secure = true // only send cookie over HTTPS
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
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;
            context.Response.Cookies.Delete(key);
        }
    }
}
