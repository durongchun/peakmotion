using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Member
{
    public int Pkemailid { get; set; }

    public DateOnly? Lastloggedin { get; set; }

    public int? Fkuserid { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual User? Fkuser { get; set; }

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
