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

    public AdminController(ProductRepo productRepo)
    {
        _productRepo = productRepo;
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

    // [HttpPost("Product/Edit")]
    public async Task<IActionResult> UploadImages(List<IFormFile> files, string photoName, string photoPath, bool isPrimary, int sortOrder, int productId)
    {
        _productRepo.UploadImagesFromAdminProductEdit(files, photoName, photoPath, isPrimary, sortOrder, productId);

        // Redirect to a relevant page (e.g., Product details page)
        return RedirectToAction("ProductEdit", new { id = 1 }); // Adjust the ID or redirect to another action as needed
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
                    // Log or display the error messages
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

        // Manually split comma separated fields if necessary
        model.Colors = _productRepo.FormatDropdownSelectedValue(Request.Form["Colors"]);
        model.Sizes = _productRepo.FormatDropdownSelectedValue(Request.Form["Sizes"]);
        model.Types = _productRepo.FormatDropdownSelectedValue(Request.Form["Types"]);
        model.Properties = _productRepo.FormatDropdownSelectedValue(Request.Form["Properties"]);



        // Update properties with new values from the model
        existingProduct.ProductName = model.ProductName;
        existingProduct.Description = model.Description;
        existingProduct.Price = model.Price;
        existingProduct.Currency = model.Currency;
        existingProduct.Quantity = model.Quantity;
        existingProduct.IsFeatured = model.IsFeatured;
        existingProduct.IsMembershipProduct = model.IsMembershipProduct;

        // Handle Categories=types, Images, Colors, Sizes
        existingProduct.Colors = model.Colors;
        existingProduct.Sizes = model.Sizes;
        existingProduct.Types = model.Types;
        existingProduct.Properties = model.Properties;

        // Update existing images
        existingProduct.Images = model.Images.ToList();


        // Handle new images
        if (NewImages != null && NewImages.Any())
        {
            foreach (var file in NewImages)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    existingProduct.Images.Add(new ProductImage
                    {
                        Url = "/images/" + fileName,
                        Alttag = Path.GetFileNameWithoutExtension(file.FileName),
                        Isprimary = false
                    });
                }
            }
        }
        //Save changes
        // _context.SaveChanges();

        // Redirect to another page (e.g., Product List)
        return RedirectToAction("ProductEdit");
    }

}





