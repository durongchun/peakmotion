using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace peakmotion.Models;

public partial class Discount
{
    public int Pkdiscountid { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy, HH:mm}")]
    public DateTime Expirydate { get; set; } = new DateTime();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
