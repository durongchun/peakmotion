using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;
using peakmotion.Data;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Controllers;

public class ProductController : Controller
{
    private readonly ProductRepo _productRepo;
    private readonly PeakmotionContext _context;

    public ProductController(ProductRepo productRepo, PeakmotionContext context)
    {
        _productRepo = productRepo;
        _context = context;
    }

    public IActionResult Index()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

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

        // Return the view with the view model
        return View(productDetailViewModel);
    }
}