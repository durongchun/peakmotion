using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Discount
{
    public int Pkdiscountid { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Expirydate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
