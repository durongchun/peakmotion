using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using peakmotion.Repositories;

namespace peakmotion.Controllers;

public class BaseController : Controller
{
    private readonly CookieRepo _cookieRepo;

    public BaseController(CookieRepo cookieRepo)
    {
        _cookieRepo = cookieRepo;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // Set the cart item count in ViewBag before every action
        var cartProducts = _cookieRepo.GetProductsFromCookie();
        ViewBag.CartItemCount = cartProducts.Count();
        // Console.WriteLine($"Cart Qty: {ViewBag.CartItemCount}");
        base.OnActionExecuting(filterContext);
    }


}
