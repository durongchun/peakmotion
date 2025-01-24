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
            var wishlistItems = from w in _context.Wishlists
                                join p in _context.Products on w.Fkproductid equals p.Pkproductid
                                where w.Fkpmuserid == userId
                                select new WishlistVM
                                {
                                    WishlistId = w.Pkwishlistid,
                                    ProductId = p.Pkproductid,
                                    ProductName = p.Name,
                                    Price = p.Regularprice
                                };

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
