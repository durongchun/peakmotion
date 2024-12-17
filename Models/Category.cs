using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Category
{
    public int Pkcategoryid { get; set; }

    public string? Categorygroup { get; set; }

    public string Categoryname { get; set; } = null!;

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
