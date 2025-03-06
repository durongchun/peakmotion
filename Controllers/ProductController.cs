
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
    /// <param name="category">This is a value from the topbar</param>
    /// <returns></returns>
    public IActionResult Index(string? searchString = null, string sortedByString = "A-Z", string category = "")
    {
        Console.WriteLine($"DEBUG: PRODUCT LIST (search: {searchString})");
        Console.WriteLine($"DEBUG: PRODUCT LIST (sortby: {searchString})");
        Console.WriteLine($"DEBUG: PRODUCT LIST (category: {category})");

        // Filter by topbar selection + sort (assuming sortby is valid)
        int? categoryId = _productRepo.GetFilterCategoryId(category);
        Console.WriteLine($"DEBUG: PRODUCT LIST (category id: {categoryId})");
        List<int>? filterId = categoryId.HasValue ? new List<int> { (int)categoryId } : null;
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedByString, filterId);

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
            FilterId = categoryId,
            SortOptions = _productRepo.GetSortBySelectList(),
            SortByChoice = sortedByString // if doesn't match in the list - default to A-Z on frontend
        };
        // Set the page title/breadcrum
        switch (category)
        {
            case "Men": ViewBag.PageTitle = "Men"; break;
            case "Women": ViewBag.PageTitle = "Women"; break;
            case "Equipment": ViewBag.PageTitle = "Equipment"; break;
            default: ViewBag.PageTitle = "All Products"; break;
        }
        return View(data);
    }

    /// <summary>
    ///     For PARTIAL RELOADS
    ///         - sorting 
    ///         - filtering
    ///         - rendering partial view for new product order
    /// </summary>
    /// <param name="sortedByString"></param>
    /// <param name="numbers"></param>
    /// <returns></returns>
    public ActionResult FilterAndSort(string sortedBy, string numbers)
    {
        Console.WriteLine($"Sorting products: {sortedBy}");
        Console.WriteLine($"Filtering products by category id: {numbers}");

        List<int> selectedIds = new List<int>();
        try
        {
            selectedIds = JsonSerializer.Deserialize<List<int>>(numbers);
            Console.WriteLine($"Filtering products by category id: {string.Join(", ", selectedIds)}");
        }
        catch (System.Exception)
        {
            Console.WriteLine($"JSON ERROR: Could not convert filter id to string: {numbers}");
        }

        IEnumerable<ProductVM> products = _productRepo.GetAllProducts(sortedBy, selectedIds);
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
            .Include(p => p.Fkdiscount)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Fkcategory)
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

        var productVM = new ProductVM
        {
            ID = product.Pkproductid,
            ProductName = product.Name,
            Description = product.Description ?? string.Empty,
            Price = product.Regularprice,
            Quantity = product.Qtyinstock,
            IsFeatured = product.Isfeatured == 1,
            IsMembershipProduct = product.Ismembershipproduct == 1,
            Discount = product.Fkdiscount,
            Categories = product.ProductCategories.Select(pc => pc.Fkcategory).ToList(),
            Images = product.ProductImages,
            PrimaryImage = product.ProductImages.FirstOrDefault(),
            Pkdiscountid = product.Fkdiscountid
        };

        ViewBag.ProductVM = productVM;
        // Return the view with the view model
        return View(productDetailViewModel);
    }
}