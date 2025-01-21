using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class ProductCategory
{
    public int Pkproductcategoryid { get; set; }

    public int Fkcategoryid { get; set; }

    public int Fkproductid { get; set; }

    public virtual Category Fkcategory { get; set; } = null!;

    public virtual Product Fkproduct { get; set; } = null!;
}
