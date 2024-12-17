using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Cart
{
    public int Pkcartid { get; set; }

    public int Qty { get; set; }

    public int Fkemailid { get; set; }

    public virtual ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

    public virtual Member Fkemail { get; set; } = null!;
}
