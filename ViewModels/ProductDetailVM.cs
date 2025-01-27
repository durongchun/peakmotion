using System;
using System.Collections.Generic;
using peakmotion.Models;

namespace peakmotion.ViewModels;

public partial class ProductDetailViewModel
{
    public int Pkproductid { get; set; }

    public string Productname { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Unitprice { get; set; }

    public int Qty { get; set; }

    public int Fkcategoryid { get; set; }

    public IEnumerable<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

}
