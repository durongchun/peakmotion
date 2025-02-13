using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using peakmotion.Models;
using peakmotion.Repositories;
using peakmotion.ViewModels;

namespace peakmotion.Controllers;

// [Authorize(Roles = "Admin, Employee")]
public class AdminController : Controller
{
    private readonly ProductRepo _productRepo;
    private readonly PeakmotionContext _context;

    public AdminController(ProductRepo productRepo, PeakmotionContext context)
    {
        _productRepo = productRepo;
        _context = context;
    }

    public IActionResult Products()
    {
        IEnumerable<ProductVM> products = _productRepo.GetAllProducts();
        return View(products);
    }

    public IActionResult ProductEdit(int id)
    {
        ProductVM? product = _productRepo.GetProduct(id);

        return View(product);
    }



    public IActionResult ProductDelete(int id)
    {
        ProductVM? product = _productRepo.GetProduct(id);

        return View(product);
    }

    [HttpPost, ActionName("ProductDelete")]
    public IActionResult DeleteConfirmed(int id)
    {
        string returnMessage = _productRepo.RemoveProduct(id);

        return RedirectToAction("Products",
                                new { message = returnMessage });

    }




    [HttpPost("Product/Edit")]
    public async Task<IActionResult> ProductDetailsEdit(ProductVM model, List<IFormFile> NewImages)
    {
        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                foreach (var error in ModelState[key].Errors)
                {
                    Console.WriteLine($"Error with key '{key}': {error.ErrorMessage}");
                }
            }
            return View(model);
        }

        var product = await _context.Products.FindAsync(model.ID);
        if (product == null)
        {
            return NotFound();
        }

        _productRepo.UpdateProductDetail(product, model);


        _productRepo.UpdateProductCategories(model);


        // Handle new images
        _productRepo.UploadImagesFromAdminProductEdit(model, NewImages);


        // Redirect to the product edit page
        return RedirectToAction("ProductEdit", new { id = model.ID });
    }


}





