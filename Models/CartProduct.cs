using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class CartProduct
{
    public int Pkcartproductcid { get; set; }

    public int Fkcartid { get; set; }

    public int Fkproductid { get; set; }

    public virtual Cart Fkcart { get; set; } = null!;

    public virtual Product Fkproduct { get; set; } = null!;
}
