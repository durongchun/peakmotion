using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;

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
                    join pi in _context.ProductImages on p.Pkproductid equals pi.Fkproductid into imgGroup
                    from pi in imgGroup.DefaultIfEmpty()  // Left join with images
                    join d in _context.Discounts on p.Fkdiscountid equals d.Pkdiscountid into discountGroup
                    from d in discountGroup.DefaultIfEmpty()  // Left join with discounts
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
            return images ?? new List<ProductImage>();

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

        public async Task UploadImagesFromAdminProductEdit(ProductVM model, List<IFormFile> NewImages)
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

                    var fileName = model.photoName.Replace(" ", "").Trim();
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Copy the file first
                    await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await file.CopyToAsync(stream);
                        await stream.FlushAsync(); // Ensure all bytes are written
                    }

                    // Create the image entity
                    var newImage = new ProductImage
                    {
                        Fkproductid = model.ID,
                        Url = $"/images/products/{fileName}",
                        Alttag = model.photoName,
                        Isprimary = false,
                    };

                    newImageEntities.Add(newImage);
                    existingProduct.Images.Add(newImage);
                }

                // Add all new images to context
                await _context.ProductImages.AddRangeAsync(newImageEntities);

                // Save all changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing images: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Error processing images: {ex.Message}", ex);
            }
        }

        public async Task UpdateProductDetail(Product product, ProductVM model)
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
            await _context.SaveChangesAsync(); // ✅ Await the async method
        }


        public async Task UpdateProductCategoriesAsync(ProductVM model)
        {
            try
            {
                // Remove existing category associations for the product
                var existingCategories = await _context.ProductCategories
                    .Where(pc => pc.Fkproductid == model.ID)
                    .ToListAsync();

                if (existingCategories.Any())
                {
                    _context.ProductCategories.RemoveRange(existingCategories);
                    await _context.SaveChangesAsync();
                }

                // Combine all selected category names from Types, Colors, Sizes, and Properties
                var categoryNames = (model.Types ?? new List<string>())
                    .Concat(model.Colors ?? new List<string>())
                    .Concat(model.Sizes ?? new List<string>())
                    .Concat(model.Properties ?? new List<string>())
                    .Distinct() // Remove duplicates
                    .Where(name => !string.IsNullOrEmpty(name)) // Exclude null or empty names
                    .SelectMany(name => name.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    .Select(name => name.ToLower())
                    .Distinct()
                    .ToList();

                // Log the category names to be queried
                Console.WriteLine("Category Names for Query: " + string.Join(", ", categoryNames));

                // Query the database for categories
                var categories = await _context.Categories
                    .Where(c => categoryNames.Contains(c.Categoryname.Trim().ToLower()))
                    .ToDictionaryAsync(c => c.Categoryname.ToLower(), c => c.Pkcategoryid);

                Console.WriteLine("Categories Retrieved: " + string.Join(", ", categories.Keys));

                // Create new ProductCategory entries
                var newProductCategories = categoryNames
                    .Where(category => categories.ContainsKey(category))
                    .Select(category =>
                    {
                        categories.TryGetValue(category, out var categoryId);
                        return new ProductCategory
                        {
                            Fkproductid = model.ID,
                            Fkcategoryid = categoryId
                        };
                    })
                    .ToList();

                // Bulk insert new category associations
                await _context.ProductCategories.AddRangeAsync(newProductCategories);

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle exception properly, possibly using a logger
                Console.WriteLine($"Error processing categories: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Error processing categories: {ex.Message}", ex);
            }
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

    }
}

