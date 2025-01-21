using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Wishlist
{
    public int Pkwishlistid { get; set; }

    public int Fkpmuserid { get; set; }

    public int Fkproductid { get; set; }

    public virtual Pmuser Fkpmuser { get; set; } = null!;

    public virtual Product Fkproduct { get; set; } = null!;
}
