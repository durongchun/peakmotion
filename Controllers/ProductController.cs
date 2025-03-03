
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;

using Microsoft.EntityFrameworkCore;
using System.Text.Json;


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
        Console.WriteLine($"Searching products: {searchString}");
        Console.WriteLine($"Sorting products: {sortedByString}");

        // Find all the category types the product can be filtered by
        Dictionary<string, List<Category>> filterTypes = _productRepo.CreateDictionaryOfCategories();

        // Display the options for sorting the products
        ViewBag.SortOptions = _productRepo.GetSortBySelectList();

        // Find all the products (assuming sort by is valid)
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedByString);

        // Filter by search query
        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(p => p.ProductName.ToLower().Contains(searchString.ToLower()));
        }
        ViewBag.SearchString = searchString;

        // Build the response
        CategoriesProductsVM data = new CategoriesProductsVM
        {
            Products = products,
            Filters = filterTypes,
            SortByChoice = sortedByString
        };
        ViewBag.PageTitle = "All Products";
        return View(data);
    }

    // For sorting and rendering partial view for new product order
    public ActionResult SortProducts(string sortedByString = "A-Z")
    {
        Console.WriteLine($"Sorting products: {sortedByString}");

        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedByString);
        return PartialView("Product/_ProductList", products);
    }

    // For sorting and rendering partial view for new product order
    public ActionResult FilterProducts(string sortedByString, string numbers)
    {
        Console.WriteLine($"Sorting products: {sortedByString}");
        Console.WriteLine($"Filtering products by category id: {numbers}");

        List<int> selectedIds = JsonSerializer.Deserialize<List<int>>(numbers);
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedByString, selectedIds);
        return PartialView("Product/_ProductList", products);
    }

    [HttpGet("/Product/{topBarFilter}")]
    // This is used from the topbar
    public IActionResult Index(string topBarFilter)
    {
        Console.WriteLine($"Products from topbar: {topBarFilter}");

        // Verify the name is allowed
        string[] allowedFilters = ["Male", "Female", "Gear"];
        List<int> selectedId = [];
        if (allowedFilters.Contains(topBarFilter))
        {
            int? id = _productRepo.GetCategoryIdByName(topBarFilter);
            if (id != null) selectedId.Add((int)id);
        }

        // Find all the category types the product can be filtered by
        Dictionary<string, List<Category>> filterTypes = _productRepo.CreateDictionaryOfCategories();

        // Display the options for sorting the products
        ViewBag.SortOptions = _productRepo.GetSortBySelectList();

        // Filter product
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts("A-Z", selectedId);

        // Build response
        CategoriesProductsVM data = new CategoriesProductsVM
        {
            Products = products,
            Filters = filterTypes,
            SortByChoice = "A-Z"
        };
        ViewBag.PageTitle = topBarFilter;
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