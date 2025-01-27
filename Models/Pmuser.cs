using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Pmuser
{
    public int Pkpmuserid { get; set; }

    public string Lastname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Province { get; set; }

    public string? Postalcode { get; set; }

    public string? Country { get; set; }

    public string Email { get; set; } = null!;

    public DateOnly? Lastloggedin { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
