using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;
using peakmotion.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


namespace peakmotion.Controllers;

public class ProductController : Controller
{
    private readonly ProductRepo _productRepo;
    private readonly PeakmotionContext _context;
    private readonly SessionRepo _sessionRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductController(ProductRepo productRepo, PeakmotionContext context, SessionRepo sessionRepo, IHttpContextAccessor httpContextAccessor)
    {
        _productRepo = productRepo;
        _context = context;
        _sessionRepo = sessionRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

    // public IActionResult Details()
    // {
    //     // Create a sample ProductDetailViewModel with placeholder data
    //     var product = new ProductDetailViewModel
    //     {
    //         Pkproductid = 1,
    //         Productname = "Sample Product",
    //         Description = "This is a sample product description used for styling.",
    //         Unitprice = 100.00m,
    //         Qty = 10,
    //         Fkcategoryid = 1,
    //         Fkcategory = new Category
    //         {
    //             Pkcategoryid = 1,
    //             Categoryname = "Sample Category"
    //         },
    //         OrderProducts = new List<OrderProduct>()
    //     };

    //     return View(product);
    // }


    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        // Fetch the product with the given ID from the database
        var product = await _context.Products
            // .Include(p => p.Fkdiscount) // Include Discount if necessary
            .FirstOrDefaultAsync(p => p.Pkproductid == id);

        // If no product is found, return NotFound
        if (product == null)
        {
            return NotFound();
        }

        // Map the database model to the view model
        var productDetailViewModel = new ProductDetailViewModel
        {
            Pkproductid = product.Pkproductid,
            Productname = product.Name,
            Description = product.Description ?? string.Empty,
            Unitprice = product.Regularprice,
            Qty = product.Qtyinstock,
            Fkcategoryid = 0,
            // Fkcategory = null
        };

        _sessionRepo.SetProductDataToSession();



        // Return the view with the view model
        return View(productDetailViewModel);
    }



}