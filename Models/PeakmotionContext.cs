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

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderProduct> OrderProducts { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Pmuser> Pmusers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //         => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=peakmotion;Username=postgres;Password=P@ssw0rd!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Pkorderid).HasName("Order_pkey");

            entity.ToTable("Order");

            entity.Property(e => e.Pkorderid).HasColumnName("pkorderid");
            entity.Property(e => e.Deliverydate).HasColumnName("deliverydate");
            entity.Property(e => e.Fkpmuserid).HasColumnName("fkpmuserid");
            entity.Property(e => e.Orderdate).HasColumnName("orderdate");
            entity.Property(e => e.Pptransactionid).HasColumnName("pptransactionid");
            entity.Property(e => e.Shippeddate).HasColumnName("shippeddate");

            entity.HasOne(d => d.Fkpmuser).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Fkpmuserid)
                .HasConstraintName("Order_fkpmuserid_fkey");
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
            entity.Property(e => e.Fkorderid).HasColumnName("fkorderid");
            entity.Property(e => e.Orderstate)
                .HasMaxLength(50)
                .HasColumnName("orderstate");

            entity.HasOne(d => d.Fkorder).WithMany(p => p.OrderStatuses)
                .HasForeignKey(d => d.Fkorderid)
                .HasConstraintName("OrderStatus_fkorderid_fkey");
        });

        modelBuilder.Entity<Pmuser>(entity =>
        {
            entity.HasKey(e => e.Pkpmuserid).HasName("PMUser_pkey");

            entity.ToTable("PMUser");

            entity.HasIndex(e => e.Email, "PMUser_email_key").IsUnique();

            entity.Property(e => e.Pkpmuserid).HasColumnName("pkpmuserid");
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
            entity.Property(e => e.Lastloggedin).HasColumnName("lastloggedin");
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

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Pkwishlistid).HasName("Wishlist_pkey");

            entity.ToTable("Wishlist");

            entity.Property(e => e.Pkwishlistid).HasColumnName("pkwishlistid");
            entity.Property(e => e.Fkpmuserid).HasColumnName("fkpmuserid");
            entity.Property(e => e.Fkproductid).HasColumnName("fkproductid");

            entity.HasOne(d => d.Fkpmuser).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.Fkpmuserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Wishlist_fkpmuserid_fkey");

            entity.HasOne(d => d.Fkproduct).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.Fkproductid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Wishlist_fkproductid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
