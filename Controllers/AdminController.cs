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

        var existingProduct = _productRepo.GetProduct(model.ID);

        if (existingProduct == null)
        {
            return NotFound();
        }

        model.Colors = _productRepo.FormatDropdownSelectedValue(Request.Form["Colors"]);
        model.Sizes = _productRepo.FormatDropdownSelectedValue(Request.Form["Sizes"]);
        model.Types = _productRepo.FormatDropdownSelectedValue(Request.Form["Types"]);
        model.Properties = _productRepo.FormatDropdownSelectedValue(Request.Form["Properties"]);

        existingProduct.ProductName = model.ProductName;
        existingProduct.Description = model.Description;
        existingProduct.Price = model.Price;
        existingProduct.Currency = model.Currency;
        existingProduct.Quantity = model.Quantity;
        existingProduct.IsFeatured = model.IsFeatured;
        existingProduct.IsMembershipProduct = model.IsMembershipProduct;
        existingProduct.Colors = model.Colors;
        existingProduct.Sizes = model.Sizes;
        existingProduct.Types = model.Types;
        existingProduct.Properties = model.Properties;
        existingProduct.Images = model.Images.ToList();

        // Handle new images
        _productRepo.UploadImagesFromAdminProductEdit(model, NewImages, existingProduct);


        // Redirect to the product edit page
        return RedirectToAction("ProductEdit", new { id = model.ID });
    }


}





