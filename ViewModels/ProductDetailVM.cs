using System;
using System.Collections.Generic;
using peakmotion.Models;

namespace peakmotion.ViewModels;

public partial class ProductDetailVM
{
    public int Pkproductid { get; set; }

    public string Productname { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Unitprice { get; set; }

    public int Qty { get; set; }

    public int Fkcategoryid { get; set; }

    public IEnumerable<String> Colors { get; set; } = new List<String>();

    public IEnumerable<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public IEnumerable<String> Sizes { get; set; } = new List<String>();

    public IEnumerable<String> Images { get; set; } = new List<String>();

}
