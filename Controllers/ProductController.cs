using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;

public class ProductController : Controller
{
    public IActionResult Details()
    {
        // Create a sample ProductDetailViewModel with placeholder data
        var product = new ProductDetailViewModel
        {
            Pkproductid = 1,
            Productname = "Sample Product",
            Description = "This is a sample product description used for styling.",
            Unitprice = 100.00m,
            Qty = 10,
            Fkcategoryid = 1,
            Fkcategory = new Category
            {
                Pkcategoryid = 1,
                Categoryname = "Sample Category"
            },
            OrderProducts = new List<OrderProduct>()
        };

        return View(product);
    }
}
