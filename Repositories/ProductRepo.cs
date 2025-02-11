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

        public async Task UploadImagesFromAdminProductEdit(ProductVM model, List<IFormFile> NewImages, ProductVM existingProduct)
        {
            // Handle new images
            if (NewImages != null && NewImages.Any())
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                existingProduct.Images = _context.ProductImages
                            .Where(img => img.Fkproductid == model.ID) // Get images for this product
                            .ToList();

                foreach (var file in NewImages)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existingProduct.Images.Add(new ProductImage
                        {
                            Fkproductid = model.ID,
                            Url = "/images/products/" + fileName,
                            Alttag = Path.GetFileNameWithoutExtension(file.FileName),
                            Isprimary = false
                        });
                    }

                    _context.SaveChanges();
                }
            }
        }

    }
}

