using peakmotion.Data;
using peakmotion.ViewModels;
using peakmotion.Models;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Repositories
{
    public class WishlistRepo
    {
        private readonly PeakmotionContext _context;

        public WishlistRepo(PeakmotionContext context)
        {
            _context = context;
        }

        public IEnumerable<WishlistVM> GetWishlistByUserId(int userId)
        {
            var query =
                from w in _context.Wishlists
                join p in _context.Products
                     on w.Fkproductid equals p.Pkproductid
                join img in _context.ProductImages
                     on p.Pkproductid equals img.Fkproductid
                     into productImagesGroup
                where w.Fkpmuserid == userId
                select new
                {
                    w.Pkwishlistid,
                    p.Pkproductid,
                    p.Name,
                    p.Regularprice,
                    ProductImages = productImagesGroup
                };

            var wishlistItems = query.AsEnumerable().Select(x =>
            {
                var primaryImg = x.ProductImages.FirstOrDefault(i => i.Isprimary);

                return new WishlistVM
                {
                    WishlistId   = x.Pkwishlistid,
                    ProductId    = x.Pkproductid,
                    ProductName  = x.Name,
                    Price        = x.Regularprice,
                    Images       = x.ProductImages.ToList(),
                    PrimaryImage = primaryImg
                };
            });

            return wishlistItems;
        }

        public void AddToWishlist(int userId, int productId)
        {
            bool exists = _context.Wishlists
                .Any(w => w.Fkpmuserid == userId && w.Fkproductid == productId);

            if (!exists)
            {
                var wishlistItem = new Wishlist
                {
                    Fkpmuserid = userId,
                    Fkproductid = productId
                };

                _context.Wishlists.Add(wishlistItem);
                _context.SaveChanges();
            }
        }

        public void RemoveFromWishlist(int userId, int productId)
        {
            var wishlistItem = _context.Wishlists
                .FirstOrDefault(w => w.Fkpmuserid == userId && w.Fkproductid == productId);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
                _context.SaveChanges();
            }
        }
    }
}
