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

    public int Isfeatured { get; set; }

    public int Ismembershipproduct { get; set; }

    public int? Fkdiscountid { get; set; }

    public virtual Discount? Fkdiscount { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
