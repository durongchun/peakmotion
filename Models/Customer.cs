using System;
using System.Collections.Generic;

namespace peakmotion.Models;

public partial class Customer
{
    public int Pkuserid { get; set; }

    public string Lastname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string Postalcode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Usertype { get; set; } = null!;

    public DateOnly? Lastloggedin { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
