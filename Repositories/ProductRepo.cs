using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

namespace peakmotion.Repositories
{
    public class ProductRepo
    {
        private readonly PeakmotionContext _context;

        public ProductRepo(PeakmotionContext context)
        {
            _context = context;
        }

        // Get specific product in the database.
        public ProductVM? GetProduct(int id)
        {
            // Fetch product details
            var product = FetchProductFromDb(id);
            if (product == null)
            {
                return null;  // Return null if no product is found
            }

            // Fetch attributes
            var colors = GetProductAttributes(id, "color");
            var sizes = GetProductAttributes(id, "size");
            var types = GetProductAttributes(id, "category");
            var colordropdown = FetchCategoryDropdown("color");
            var sizedropdown = FetchCategoryDropdown("size");
            var categorydropdown = FetchCategoryDropdown("category");

            // Map to ViewModel
            var productVM = new ProductVM
            {
                ID = product.Pkproductid,
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Regularprice,
                Quantity = product.Qtyinstock,
                IsFeatured = product.Isfeatured == 1,
                IsMembershipProduct = product.Ismembershipproduct == 1,
                Discount = product.Fkdiscount,
                Colors = colors,
                Sizes = sizes,
                Types = types,
                ColorDropdown = colordropdown,
                SizeDropdown = sizedropdown,
                TypeDropdown = categorydropdown,


            };

            return productVM;
        }

        public List<Category> FetchCategoryDropdown(string categoryGroup)
        {

            return _context.Categories
                           .Where(c => c.Categorygroup == categoryGroup)
                           .ToList();
        }

        public Product? FetchProductFromDb(int id)
        {
            return (from p in _context.Products
                    join pi in _context.ProductImages on p.Pkproductid equals pi.Fkproductid
                    join d in _context.Discounts on p.Fkdiscountid equals d.Pkdiscountid
                    where p.Pkproductid == id
                    select new Product
                    {
                        Pkproductid = p.Pkproductid,
                        Name = p.Name,
                        Description = p.Description,
                        Regularprice = p.Regularprice,
                        Qtyinstock = p.Qtyinstock,
                        Isfeatured = p.Isfeatured,
                        Ismembershipproduct = p.Ismembershipproduct,
                        Fkdiscountid = p.Fkdiscountid
                    }).FirstOrDefault();
        }

        // Generalized method to fetch product attributes
        public List<string> GetProductAttributes(int id, string categoryGroup)
        {
            return (from pc in _context.ProductCategories
                    join c in _context.Categories on pc.Fkcategoryid equals c.Pkcategoryid
                    where pc.Fkproductid == id && c.Categorygroup == categoryGroup
                    select c.Categoryname).ToList();
        }


        // Get all products in the database.
        public IEnumerable<ProductVM> GetAllProducts()
        {
            IEnumerable<ProductVM> products = from p in _context.Products
                                              join pi in _context.ProductImages on p.Pkproductid equals pi.Fkproductid into pImageGroup
                                              from pi in pImageGroup.DefaultIfEmpty()
                                              join d in _context.Discounts on p.Fkdiscountid equals d.Pkdiscountid into productGroup
                                              from d in productGroup.DefaultIfEmpty()
                                              select new ProductVM
                                              {
                                                  ID = p.Pkproductid,
                                                  ProductName = p.Name,
                                                  Description = p.Description,
                                                  Price = p.Regularprice,
                                                  Quantity = p.Qtyinstock,
                                                  IsFeatured = p.Isfeatured == 1,
                                                  IsMembershipProduct = p.Ismembershipproduct == 1,
                                                  Discount = p.Fkdiscount,
                                                  Categories = p.ProductCategories
                                                        .Where(pc => pc.Fkproductid == p.Pkproductid)
                                                        .Select(x => new Category
                                                        {
                                                            Pkcategoryid = x.Fkcategoryid,
                                                            Categorygroup = x.Fkcategory.Categorygroup,
                                                            Categoryname = x.Fkcategory.Categoryname,
                                                        })
                                                        .ToList(),
                                                  Images = p.ProductImages,
                                                  PrimaryImage = p.ProductImages
                                                        .Where(pi => pi.Fkproductid == p.Pkproductid && pi.Isprimary)
                                                        .FirstOrDefault()
                                              };
            Console.WriteLine($"DEBUG: Product Count: {products.Count()}");
            return products;
        }

        public String RemoveProduct(int id)
        {
            Product? product = _context.Products
                                .Where(i => i.Pkproductid == id)
                                .FirstOrDefault();

            if (product == null)
            {
                return $"warning,Unable to find product ID: {id}";
            }

            try
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                return $"success,Successfully deleted product ID: {id}";
            }
            catch (Exception ex)
            {
                return $"error,Product could not be deleted: {ex.Message}";
            }
        }

        public async Task UploadImagesFromAdminProductEdit(List<IFormFile> files, string photoName, string photoPath, bool isPrimary, int sortOrder, int productId)
        {
            try
            {
                if (files != null && files.Count > 0)
                {
                    // Save each uploaded file to a directory and get the path
                    foreach (var file in files)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", file.FileName);

                        // Ensure directory exists
                        var directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Save the file asynchronously
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Create a new Image record and add it to the database
                        var image = new ProductImage
                        {
                            Url = filePath,   // Store the file path
                            Isprimary = isPrimary,
                            Fkproductid = productId,
                        };

                        _context.ProductImages.Add(image);
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (You can use any logging framework like Serilog, NLog, etc.)
                // For example: _logger.LogError(ex, "An error occurred while uploading images.");

                // Optionally, rethrow or handle the exception (return an error response, etc.)
                throw new Exception("An error occurred while uploading images.", ex);
            }
        }

    }
}

