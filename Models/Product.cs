using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Product
{
    public int Pkproductid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Regularprice { get; set; }

    public int Qtyinstock { get; set; }

    public bool Isfeatured { get; set; }

    public int Ismembershipproduct { get; set; }

    public int Fkdiscountid { get; set; }

    public virtual ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

    public virtual Discount Fkdiscount { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
