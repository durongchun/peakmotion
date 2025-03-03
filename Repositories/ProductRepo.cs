﻿using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var discountDropdown = _context.Discounts.ToList();
            // Find the selected discount's description
            var selectedDiscountDescription = discountDropdown
                .FirstOrDefault(d => d.Pkdiscountid == product.Fkdiscountid)?.Description;
            var PrimaryImageUrl = GetProductPrimaryImage(id);




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
                SelectedDiscountId = product.Fkdiscountid,
                DiscountDropdown = discountDropdown,
                SelectedDiscountDescription = selectedDiscountDescription,
                ImageUrl = PrimaryImageUrl,


            };

            return productVM;
        }

        public List<Category> FetchCategoryDropdown(string categoryGroup)
        {

            return _context.Categories
                           .Where(c => c.Categorygroup == categoryGroup)
                           .ToList();
        }

        public int? GetCategoryIdByName(string name)
        {
            Category? category = _context.Categories
                           .Where(c => c.Categoryname.ToLower() == name.ToLower())
                           .FirstOrDefault();
            return category?.Pkcategoryid;
        }

        public Dictionary<string, List<Category>> CreateDictionaryOfCategories()
        {
            // Find all the category types the product can be filtered by
            var genderChoices = FetchCategoryDropdown("gender");
            var colorChoices = FetchCategoryDropdown("color");
            var sizeChoices = FetchCategoryDropdown("size");
            var categoryChoices = FetchCategoryDropdown("category");
            var propertyChoices = FetchCategoryDropdown("property");
            Dictionary<string, List<Category>> filterTypes = new Dictionary<string, List<Category>>
            {
                { "category", categoryChoices },
                { "property", propertyChoices },
                { "gender", genderChoices },
                { "color", colorChoices },
                { "size", sizeChoices }
            };

            return filterTypes;
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
                        Pkimageid = img.Pkimageid,
                        Fkproductid = id,
                        Url = img.Url,
                        Isprimary = img.Isprimary,
                        Alttag = img.Alttag,
                    })
                    .ToList();
            return images ?? new List<ProductImage>();

        }

        public string GetProductPrimaryImage(int id)
        {
            var primaryImage = _context.ProductImages
                                .Where(img => img.Fkproductid == id && img.Isprimary)
                                .Select(img => new ProductImage
                                {
                                    Pkimageid = img.Pkimageid,
                                    Fkproductid = id,
                                    Url = img.Url,
                                    Isprimary = img.Isprimary,
                                    Alttag = img.Alttag,
                                })
                                .FirstOrDefault();

            return primaryImage?.Url ?? "https://picsum.photos/500/700?random=1";
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

        public async Task UploadImagesFromAdminProductEdit(ProductVM model, List<IFormFile> NewImages)
        {
            try
            {
                // Delete images if any are marked for deletion from page
                if (!string.IsNullOrEmpty(model.ImagesToDelete))
                {
                    await DeleteProductImages(model.ID, model.ImagesToDelete);
                }

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
                        Isprimary = model.Isprimary,
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
            product.Fkdiscountid = model.SelectedDiscountId;

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

        public async Task DeleteProductImages(int productId, string imageIdsToDelete)
        {
            if (string.IsNullOrEmpty(imageIdsToDelete))
                return;

            // Parse the comma-separated IDs
            var imageIds = imageIdsToDelete.Split(',')
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => int.Parse(id))
                .ToList();

            if (!imageIds.Any())
                return;


            // Get images to delete
            var imagesToDelete = await _context.ProductImages
                .Where(img => img.Fkproductid == productId && imageIds.Contains(img.Pkimageid))
                .ToListAsync();

            if (!imagesToDelete.Any())
                return;

            // Get file paths to delete from disk
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filesToDelete = imagesToDelete
                .Select(img => Path.Combine(uploadPath, img.Url.TrimStart('/')))
                .Where(path => File.Exists(path))
                .ToList();

            // Remove from database
            _context.ProductImages.RemoveRange(imagesToDelete);
            await _context.SaveChangesAsync();

            // Delete physical files
            foreach (var filePath in filesToDelete)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
                }
            }
        }

        public string RemoveProduct(int id)
        {
            try
            {
                // Fetch the product from the database
                var product = _context.Products
                    .Include(p => p.ProductImages) // Include related images
                    .Include(p => p.ProductCategories) // Include related categories
                    .Include(p => p.OrderProducts) // Include related order products
                    .Include(p => p.Wishlists) // Include related wishlists
                    .FirstOrDefault(p => p.Pkproductid == id);

                if (product == null)
                {
                    return "Product not found.";
                }

                // Delete associated images (if any)
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    foreach (var image in product.ProductImages)
                    {
                        var filePath = Path.Combine(uploadPath, image.Url.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            // Delete the physical file
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Delete images from the database
                    _context.ProductImages.RemoveRange(product.ProductImages);
                }

                // Delete associated categories (if any)
                if (product.ProductCategories != null && product.ProductCategories.Any())
                {
                    _context.ProductCategories.RemoveRange(product.ProductCategories);
                }

                // Delete associated order products (if any)
                if (product.OrderProducts != null && product.OrderProducts.Any())
                {
                    _context.OrderProducts.RemoveRange(product.OrderProducts);
                }

                // Delete associated wishlists (if any)
                if (product.Wishlists != null && product.Wishlists.Any())
                {
                    _context.Wishlists.RemoveRange(product.Wishlists);
                }

                _context.Products.Remove(product);
                _context.SaveChanges();

                return "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return "An error occurred while deleting the product.";
            }
        }

        public IEnumerable<ProductVM> sortProducts(IEnumerable<ProductVM> products, string? sortBy = "")
        {
            Console.WriteLine($"Sorting results by {sortBy}");

            IEnumerable<ProductVM> sortedProducts = products;
            switch (sortBy)
            {
                case "Featured":
                    sortedProducts = products.OrderBy(p => p.IsFeatured).ToList();
                    break;
                case "A-Z":
                    sortedProducts = products.OrderBy(p => p.ProductName).ToList();
                    break;
                case "Z-A":
                    sortedProducts = products.OrderByDescending(p => p.ProductName).ToList();
                    break;
                case "Price: High to Low":
                    sortedProducts = products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "Price: Low to High":
                    sortedProducts = products.OrderBy(p => p.Price).ToList();
                    break;
                default:
                    sortedProducts = products.OrderBy(p => p.ProductName).ToList();
                    break;
            }
            Console.WriteLine($"DEBUG: Sorted Product Count: {sortedProducts.Count()}");
            return sortedProducts;
        }

        public IEnumerable<ProductVM> filterProducts(IEnumerable<ProductVM> products, List<int> categoryIds)
        {
            foreach (var number in categoryIds) Console.WriteLine($"DEBUG: Filtering by category id: {number}");

            IEnumerable<ProductVM> filteredProducts = products.Where(p =>
            {
                var catIDs = p.Categories.Select(c => c.Pkcategoryid);
                var isInFilter = false;
                foreach (var id in catIDs)
                {
                    if (categoryIds.Contains(id))
                    {
                        isInFilter = true;
                        break;
                    }
                }
                return isInFilter;
            }
            );

            Console.WriteLine($"DEBUG: Filtered Product Count: {filteredProducts.Count()}");
            return filteredProducts;
        }

        // Get all products in the database.
        public IEnumerable<ProductVM> GetAllProducts(string sortBy = "A-Z", List<int>? categoryIds = null)
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

            // Sort products
            IEnumerable<ProductVM> sortedProducts = sortedProducts = sortProducts(products, sortBy);

            // Check if products need to be filtered
            if (categoryIds != null && categoryIds.Count() > 0) return filterProducts(sortedProducts, categoryIds);

            // Return all the products
            Console.WriteLine($"DEBUG: Product Count: {sortedProducts.Count()}");
            return sortedProducts;
        }

        public SelectList GetSortBySelectList()
        {
            List<string> sortByOptions = new List<string> { "Featured", "A-Z", "Z-A", "Price: High to Low", "Price: Low to High" };
            List<SelectListItem> result = sortByOptions.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();

            return new SelectList(result, "Value", "Text");
        }

        // Specifically for the Product Top bar filter
        public int? GetFilterCategoryId(string name)
        {
            // update here if the DB values are different
            Dictionary<string, string> allowedFilters = new Dictionary<string, string>
            {
                {"Men", "male"},
                {"Women", "female"},
                {"Equipment", "gear"}
            };
            string? value;
            if (allowedFilters.TryGetValue(name, out value))
            {
                Console.WriteLine($"DEBUG: Found category '{name}' with db value: {value}");
                int? id = GetCategoryIdByName(value);
                if (id != null) return id;
            }
            return null;
        }
    }
}

