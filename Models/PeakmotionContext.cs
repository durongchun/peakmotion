using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace peakmotion.Models;

public partial class PeakmotionContext : DbContext
{
    public PeakmotionContext()
    {
    }

    public PeakmotionContext(DbContextOptions<PeakmotionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartProduct> CartProducts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderProduct> OrderProducts { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=peakmotion;Username=postgres;Password=P@ssw0rd!;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Pkcartid).HasName("Cart_pkey");

            entity.ToTable("Cart");

            entity.Property(e => e.Pkcartid).HasColumnName("pkcartid");
            entity.Property(e => e.Fkemailid).HasColumnName("fkemailid");
            entity.Property(e => e.Qty).HasColumnName("qty");

            entity.HasOne(d => d.Fkemail).WithMany(p => p.Carts)
                .HasForeignKey(d => d.Fkemailid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Cart_fkemailid_fkey");
        });

        modelBuilder.Entity<CartProduct>(entity =>
        {
            entity.HasKey(e => e.Pkcartproductcid).HasName("Cart_Product_pkey");

            entity.ToTable("Cart_Product");

            entity.Property(e => e.Pkcartproductcid).HasColumnName("pkcartproductcid");
            entity.Property(e => e.Fkcartid).HasColumnName("fkcartid");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");

            entity.HasOne(d => d.Fkcart).WithMany(p => p.CartProducts)
                .HasForeignKey(d => d.Fkcartid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Cart_Product_fkcartid_fkey");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.CartProducts)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Cart_Product_fkproductid_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Pkcategoryid).HasName("Category_pkey");

            entity.ToTable("Category");

            entity.Property(e => e.Pkcategoryid).HasColumnName("pkcategoryid");
            entity.Property(e => e.Categorygroup)
                .HasMaxLength(50)
                .HasColumnName("categorygroup");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(50)
                .HasColumnName("categoryname");
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Pkdiscountid).HasName("Discount_pkey");

            entity.ToTable("Discount");

            entity.Property(e => e.Pkdiscountid).HasColumnName("pkdiscountid");
            entity.Property(e => e.Amount)
                .HasPrecision(19)
                .HasColumnName("amount");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Expirydate).HasColumnName("expirydate");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Pkemailid).HasName("Member_pkey");

            entity.ToTable("Member");

            entity.Property(e => e.Pkemailid).HasColumnName("pkemailid");
            entity.Property(e => e.Fkuserid).HasColumnName("fkuserid");
            entity.Property(e => e.Lastloggedin).HasColumnName("lastloggedin");

            entity.HasOne(d => d.Fkuser).WithMany(p => p.Members)
                .HasForeignKey(d => d.Fkuserid)
                .HasConstraintName("Member_fkuserid_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Pkorderid).HasName("Order_pkey");

            entity.ToTable("Order");

            entity.Property(e => e.Pkorderid).HasColumnName("pkorderid");
            entity.Property(e => e.Deliverydate).HasColumnName("deliverydate");
            entity.Property(e => e.Fkuserid).HasColumnName("fkuserid");
            entity.Property(e => e.Orderdate).HasColumnName("orderdate");
            entity.Property(e => e.Pptransactionid).HasColumnName("pptransactionid");
            entity.Property(e => e.Shippeddate).HasColumnName("shippeddate");

            entity.HasOne(d => d.Fkuser).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Fkuserid)
                .HasConstraintName("Order_fkuserid_fkey");
        });

        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => e.Pkorderproductid).HasName("Order_Product_pkey");

            entity.ToTable("Order_Product");

            entity.Property(e => e.Pkorderproductid).HasColumnName("pkorderproductid");
            entity.Property(e => e.Fkorderid).HasColumnName("fkorderid");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.Unitprice)
                .HasPrecision(19, 2)
                .HasColumnName("unitprice");

            entity.HasOne(d => d.Fkorder).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.Fkorderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_Product_fkorderid_fkey");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_Product_fkproductid_fkey");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Pkorderstatusid).HasName("OrderStatus_pkey");

            entity.ToTable("OrderStatus");

            entity.Property(e => e.Pkorderstatusid).HasColumnName("pkorderstatusid");
            entity.Property(e => e.Fkoderid).HasColumnName("fkoderid");
            entity.Property(e => e.Orderstate)
                .HasMaxLength(50)
                .HasColumnName("orderstate");

            entity.HasOne(d => d.Fkoder).WithMany(p => p.OrderStatuses)
                .HasForeignKey(d => d.Fkoderid)
                .HasConstraintName("OrderStatus_fkoderid_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Pkproductid).HasName("Product_pkey");

            entity.ToTable("Product");

            entity.Property(e => e.Pkproductid).HasColumnName("pkproductid");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Fkdiscountid).HasColumnName("fkdiscountid");
            entity.Property(e => e.Isfeatured).HasColumnName("isfeatured");
            entity.Property(e => e.Ismembershipproduct).HasColumnName("ismembershipproduct");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Qtyinstock).HasColumnName("qtyinstock");
            entity.Property(e => e.Regularprice)
                .HasPrecision(19, 2)
                .HasColumnName("regularprice");

            entity.HasOne(d => d.Fkdiscount).WithMany(p => p.Products)
                .HasForeignKey(d => d.Fkdiscountid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Product_fkdiscountid_fkey");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Pkproductcategoryid).HasName("Product_Category_pkey");

            entity.ToTable("Product_Category");

            entity.Property(e => e.Pkproductcategoryid).HasColumnName("pkproductcategoryid");
            entity.Property(e => e.Fkcategoryid).HasColumnName("fkcategoryid");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");

            entity.HasOne(d => d.Fkcategory).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.Fkcategoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Product_Category_fkcategoryid_fkey");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Product_Category_fkproductid_fkey");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Pkimageid).HasName("ProductImage_pkey");

            entity.ToTable("ProductImage");

            entity.Property(e => e.Pkimageid).HasColumnName("pkimageid");
            entity.Property(e => e.Alttag)
                .HasMaxLength(50)
                .HasColumnName("alttag");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");
            entity.Property(e => e.Isprimary).HasColumnName("isprimary");
            entity.Property(e => e.Url)
                .HasMaxLength(50)
                .HasColumnName("url");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProductImage_fkproductid_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Pkreviewid).HasName("Review_pkey");

            entity.ToTable("Review");

            entity.Property(e => e.Pkreviewid).HasColumnName("pkreviewid");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Review_fkproductid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Pkuserid).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.Pkuserid).HasColumnName("pkuserid");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(30)
                .HasColumnName("country");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(30)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(30)
                .HasColumnName("lastname");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            entity.Property(e => e.Postalcode)
                .HasMaxLength(15)
                .HasColumnName("postalcode");
            entity.Property(e => e.Province)
                .HasMaxLength(30)
                .HasColumnName("province");
            entity.Property(e => e.Usertype)
                .HasMaxLength(20)
                .HasColumnName("usertype");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Pkwishlistid).HasName("Wishlist_pkey");

            entity.ToTable("Wishlist");

            entity.Property(e => e.Pkwishlistid).HasColumnName("pkwishlistid");
            entity.Property(e => e.Fkemailid).HasColumnName("fkemailid");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");

            entity.HasOne(d => d.Fkemail).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.Fkemailid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Wishlist_fkemailid_fkey");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Wishlist_fkproductid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
