using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class OrderProduct
{
    public int Pkorderproductid { get; set; }

    public int Qty { get; set; }

    public decimal Unitprice { get; set; }

    public int Fkorderid { get; set; }

    public int Fkproductid { get; set; }

    public virtual Order Fkorder { get; set; } = null!;

    public virtual Product Fkproduct { get; set; } = null!;
}
