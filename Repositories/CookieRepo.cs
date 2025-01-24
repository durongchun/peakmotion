using Microsoft.AspNetCore.Http;

namespace peakmotion.Repositories
{
    public class CookieRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieRepo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddCookie(string key, string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(key, value);
        }

        public string GetCookie(string key)
        {
            return _httpContextAccessor.HttpContext.Session.GetString(key);
        }

        public void RemoveCookie(string key)
        {
            _httpContextAccessor.HttpContext.Session.Remove(key);
        }
    }
}
