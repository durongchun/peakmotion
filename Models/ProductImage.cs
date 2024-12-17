using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class ProductImage
{
    public int Pkimageid { get; set; }

    public string Url { get; set; } = null!;

    public bool Isprimary { get; set; }

    public string? Alttag { get; set; }

    public int Fkproductid { get; set; }

    public virtual Product Fkproduct { get; set; } = null!;
}
