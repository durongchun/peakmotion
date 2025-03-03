
using Microsoft.AspNetCore.Mvc;
using peakmotion.Repositories;
using peakmotion.ViewModels;
using peakmotion.Models;

using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;


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

    /// <summary>
    ///     This is used to get all products and for the search results
    ///     Note: 
    ///         - 0 parameters - use both default values
    ///         - 1 parameter - values goes to searchString
    /// </summary>
    /// <param name="searchString">This is a nullable default parameter</param>
    /// <param name="sortedByString">This is an default parameter</param>
    /// <returns></returns>
    public IActionResult Index(string? searchString = null, string sortedByString = "A-Z")
    {
        Console.WriteLine($"DEBUG: PRODUCT LIST (search: {searchString})");
        Console.WriteLine($"DEBUG: PRODUCT LIST (sortby: {searchString})");

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
            Filters = _productRepo.CreateDictionaryOfCategories(),
            SortOptions = _productRepo.GetSortBySelectList(),
            SortByChoice = sortedByString // if doesn't match in the list - default to A-Z on frontend
        };
        ViewBag.PageTitle = "All Products";
        return View(data);
    }

    /// <summary>
    ///     This is used from the topbar
    /// </summary>
    /// <param name="topBarFilter"></param>
    /// <returns></returns>
    [HttpGet("/Product/{topBarFilter}")]
    public IActionResult Index(string topBarFilter)
    {
        Console.WriteLine($"DEBUG: PRODUCT LIST (filter: {topBarFilter})");

        // Filter product
        List<int>? selectedId = _productRepo.GetFilterCategoryId(topBarFilter);
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts("A-Z", selectedId);

        // Build response
        CategoriesProductsVM data = new CategoriesProductsVM
        {
            Products = products,
            Filters = _productRepo.CreateDictionaryOfCategories(),
            SortOptions = _productRepo.GetSortBySelectList(),
            SortByChoice = "A-Z"
        };
        ViewBag.PageTitle = topBarFilter;
        return View(data);
    }

    /// <summary>
    ///     For sorting and rendering partial view for new product order
    /// </summary>
    /// <param name="sortedByString"></param>
    /// <returns></returns>
    public ActionResult SortProducts(string sortedByString = "A-Z")
    {
        Console.WriteLine($"Sorting products: {sortedByString}");

        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedByString);
        return PartialView("Product/_ProductList", products);
    }

    /// <summary>
    ///     For filtering and rendering partial view for new product order
    /// </summary>
    /// <param name="sortedByString"></param>
    /// <param name="numbers"></param>
    /// <returns></returns>
    public ActionResult FilterProducts(string sortedByString, string numbers)
    {
        Console.WriteLine($"Sorting products: {sortedByString}");
        Console.WriteLine($"Filtering products by category id: {numbers}");

        List<int> selectedIds = JsonSerializer.Deserialize<List<int>>(numbers);
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedByString, selectedIds);
        return PartialView("Product/_ProductList", products);
    }

    /// <summary>
    ///     Products details
    ///     ie. GET: /Product/Details/5
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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