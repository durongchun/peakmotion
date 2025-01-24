using Microsoft.AspNetCore.Http;
using System;

namespace peakmotion.Repositories
{
    public class CookieRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _context;

        public CookieRepo(IHttpContextAccessor httpContextAccessor, HttpContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public void AddCookie(string key, string value, int expireDays = 7)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(expireDays), 
                HttpOnly = true,  // avoid JS access for security
                Secure = true,    // only HTTPS is allowed for security
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
