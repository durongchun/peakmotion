using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Review
{
    public int Pkreviewid { get; set; }

    public string? Comment { get; set; }

    public int Rating { get; set; }

    public int Fkproductid { get; set; }

    public virtual Product Fkproduct { get; set; } = null!;
}
