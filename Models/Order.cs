using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Order
{
    public int Pkorderid { get; set; }

    public DateOnly Orderdate { get; set; }

    public DateOnly? Shippeddate { get; set; }

    public DateOnly? Deliverydate { get; set; }

    public long Pptransactionid { get; set; }

    public int? Fkpmuserid { get; set; }

    public virtual Pmuser? Fkpmuser { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ICollection<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();
}
