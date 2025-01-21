using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class OrderStatus
{
    public int Pkorderstatusid { get; set; }

    public string Orderstate { get; set; } = null!;

    public int? Fkorderid { get; set; }

    public virtual Order? Fkorder { get; set; }
}
