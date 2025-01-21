using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

public class ProductController : Controller
{
    private readonly ProductRepo _productRepo;

    public ProductController(ProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    public IActionResult Index()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

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