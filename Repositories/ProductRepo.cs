using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;
using System.Linq.Expressions;

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
            var properties = GetProductAttributes(id, "property");
            var colordropdown = FetchCategoryDropdown("color");
            var sizedropdown = FetchCategoryDropdown("size");
            var categorydropdown = FetchCategoryDropdown("category");
            var propertydropdown = FetchCategoryDropdown("property");
            var productimages = GetProductImage(id);



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
                Properties = properties,
                ColorDropdown = colordropdown,
                SizeDropdown = sizedropdown,
                TypeDropdown = categorydropdown,
                PropertyDropdown = propertydropdown,
                Images = productimages,


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

        public List<ProductImage> GetProductImage(int id)
        {
            var images = _context.ProductImages
                    .Where(img => img.Fkproductid == id)
                    .Select(img => new ProductImage
                    {
                        Url = img.Url,
                        Isprimary = img.Isprimary,
                        Alttag = img.Alttag,
                    })
                    .ToList();
            return images;

        }

        public List<string> FormatDropdownSelectedValue(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return key.ToString()
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
            return new List<string>();
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

        public Task UploadImagesFromAdminProductEdit(ProductVM model, List<IFormFile> NewImages)
        {
            try
            {
                if (NewImages?.Any() != true) return;

                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                Directory.CreateDirectory(uploadPath); // Ensures the directory exists

                var existingProduct = GetProduct(model.ID);

                // Load existing images
                existingProduct.Images = await _context.ProductImages
                    .Where(img => img.Fkproductid == model.ID)
                    .ToListAsync();

                // Process all images first
                var newImageEntities = new List<ProductImage>();

                foreach (var file in NewImages)
                {
                    if (file.Length <= 0) continue;

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Copy the file first
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        file.CopyToAsync(stream);
                        stream.FlushAsync(); // Ensure all bytes are written
                    }

                    // Create the image entity
                    var newImage = new ProductImage
                    {
                        Fkproductid = model.ID,
                        Url = $"/images/products/{fileName}",
                        Alttag = Path.GetFileNameWithoutExtension(file.FileName),
                        Isprimary = false
                    };

                    newImageEntities.Add(newImage);
                    existingProduct.Images.Add(newImage);
                }

                // Add all new images to context
                _context.ProductImages.AddRangeAsync(newImageEntities);

                // Save all changes
                _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing images: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Error processing images: {ex.Message}", ex);
            }
        }

        public void UpdateProductDetail(Product product, ProductVM model)
        {

            // Update the entity with new values
            product.Name = model.ProductName;
            product.Description = model.Description;
            product.Regularprice = model.Price;
            product.Qtyinstock = model.Quantity;
            product.Isfeatured = model.IsFeatured ? 1 : 0;
            product.Ismembershipproduct = model.IsMembershipProduct ? 1 : 0;
            product.Fkdiscountid = 1;

            // Save changes
            _context.SaveChangesAsync();


        }

        public void UpdateProductCategeries(ProductVM model)
        {
            // Remove existing category associations for the product
            var existingCategories = _context.ProductCategories
                .Where(pc => pc.Fkproductid == model.ID);

            // Remove existing categories
            _context.ProductCategories.RemoveRange(existingCategories);



            // Combine all selected category names from Types, Colors, Sizes, and Properties
            var categoryNames = (model.Types ?? new List<string>())
                                    .Concat(model.Colors ?? new List<string>())
                                    .Concat(model.Sizes ?? new List<string>())
                                    .Concat(model.Properties ?? new List<string>())
                                    .Distinct() // Remove duplicates
                                    .ToList();

            // Ensure categories in the database and categoryNames are compared case-insensitively
            var categoryNamesTrimmedLower = categoryNames
                                            .Select(c => c.Trim().ToLower()) // Trim and convert to lower case for consistency
                                            .Distinct() // Ensure no duplicates in category names
                                            .ToList();

            // Log the category names for debugging (optional)
            Console.WriteLine("Category Names for Query: " + string.Join(", ", categoryNamesTrimmedLower));

            // Query the database for categories
            var categories = _context.Categories
                .Where(c => categoryNamesTrimmedLower.Contains(c.Categoryname.Trim().ToLower())) // Trim and compare case-insensitively
                .ToDictionary(c => c.Categoryname.ToLower(), c => c.Pkcategoryid);

            // Log the categories for debugging (optional)
            Console.WriteLine("Categories Retrieved: " + string.Join(", ", categories.Keys));


            // Create new ProductCategory entries
            var newProductCategories = categoryNamesTrimmedLower
                .Where(category => categories.ContainsKey(category)) // Ensure category exists in dictionary
                .Select(category => new ProductCategory
                {
                    Fkproductid = model.ID,
                    Fkcategoryid = categories[category]
                })
                .ToList();

            // Bulk insert new category associations
            _context.ProductCategories.AddRangeAsync(newProductCategories);

            // Save changes to the database
            _context.SaveChangesAsync();

        }

    }
}

