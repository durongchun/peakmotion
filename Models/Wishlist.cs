using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Wishlist
{
    public int Pkwishlistid { get; set; }

    public int Fkuserid { get; set; }

    public int Fkproductid { get; set; }

    public virtual Product Fkproduct { get; set; } = null!;

    public virtual User Fkuser { get; set; } = null!;
}
