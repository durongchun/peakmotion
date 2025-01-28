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

    public IActionResult Index()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = (from p in _context.Products
                       where p.Pkproductid == id
                       select new ProductDetailVM
                       {
                           Pkproductid = p.Pkproductid,
                           Productname = p.Name,
                           Description = p.Description ?? "",
                           Unitprice = p.Regularprice,
                           Qty = p.Qtyinstock,
                           // We'll populate Colors and Sizes separately
                       }).FirstOrDefault();
        if (product != null)
        {
            // Get all colors for this product
            product.Colors = (from pc in _context.ProductCategories
                              join c in _context.Categories on pc.Fkcategoryid equals c.Pkcategoryid
                              where pc.Fkproductid == id
                              && c.Categorygroup.ToLower() == "color"
                              select c.Categoryname).ToList();
            // Get all sizes for this product
            product.Sizes = (from pc in _context.ProductCategories
                             join c in _context.Categories on pc.Fkcategoryid equals c.Pkcategoryid
                             where pc.Fkproductid == id
                             && c.Categorygroup.ToLower() == "size"
                             select c.Categoryname).ToList();

            product.Images = (from pi in _context.ProductImages
                              where pi.Fkproductid == id
                              select pi.Url).ToList();
        }


        // var product = await _context.Products

        //     .FirstOrDefaultAsync(p => p.Pkproductid == id);


        // if (product == null)
        // {
        //     return NotFound();
        // }


        // var productDetailViewModel = new ProductDetailVM
        // {
        //     Pkproductid = product.Pkproductid,
        //     Productname = product.Name,
        //     Description = product.Description ?? string.Empty,
        //     Unitprice = product.Regularprice,
        //     Qty = product.Qtyinstock,
        //     Fkcategoryid = 0,

        // };
        _cookieRepo.SetProductDataToSession();


        return View(product);
    }
}