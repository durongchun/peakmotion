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
    private readonly CookieRepo _cookieRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductController(ProductRepo productRepo, PeakmotionContext context, CookieRepo cookieRepo, IHttpContextAccessor httpContextAccessor)
    {
        _productRepo = productRepo;
        _context = context;
        _cookieRepo = cookieRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index(string? searchString = null, string? sortedByString = "A-Z")
    {
        // Find all the category types the product can be filtered by
        var genderChoices = _productRepo.FetchCategoryDropdown("gender");
        var colorChoices = _productRepo.FetchCategoryDropdown("color");
        var sizeChoices = _productRepo.FetchCategoryDropdown("size");
        var categoryChoices = _productRepo.FetchCategoryDropdown("category");
        var propertyChoices = _productRepo.FetchCategoryDropdown("property");
        Dictionary<string, List<Category>> filterTypes = new Dictionary<string, List<Category>>
        {
            { "category", categoryChoices },
            { "property", propertyChoices },
            { "gender", genderChoices },
            { "color", colorChoices },
            { "size", sizeChoices }
        };

        // Find all the products
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(p => p.ProductName.ToLower().Contains(searchString.ToLower()));
        }

        // Build the response
        ViewBag.SearchString = searchString;
        ViewBag.SortedBy = sortedByString;
        CategoriesProductsVM data = new CategoriesProductsVM
        {
            Products = products,
            Filters = filterTypes
        };
        return View(data);
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
        var productDetailViewModel = new ProductDetailVM
        {
            Pkproductid = product.Pkproductid,
            Productname = product.Name,
            Description = product.Description ?? string.Empty,
            Unitprice = product.Regularprice,
            Qty = product.Qtyinstock,
            Fkcategoryid = 0,
            // Fkcategory = null
        };

        _cookieRepo.SetProductDataToCookie(productDetailViewModel);

        // Return the view with the view model
        return View(productDetailViewModel);
    }
}