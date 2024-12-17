using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Wishlist
{
    public int Pkwishlistid { get; set; }

    public int Fkemailid { get; set; }

    public int Fkproductid { get; set; }

    public virtual Member Fkemail { get; set; } = null!;

    public virtual Product Fkproduct { get; set; } = null!;
}
