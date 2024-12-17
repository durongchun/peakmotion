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

    public virtual DbSet<Store> Stores { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=peakmotion;Username=postgres;Password=P@ssw0rd!;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StorId).HasName("pk_store_id");

            entity.ToTable("store");

            entity.Property(e => e.StorId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("stor_id");
            entity.Property(e => e.City)
                .HasMaxLength(20)
                .HasColumnName("city");
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("state");
            entity.Property(e => e.StorAddress)
                .HasMaxLength(40)
                .HasColumnName("stor_address");
            entity.Property(e => e.StorName)
                .HasMaxLength(40)
                .HasColumnName("stor_name");
            entity.Property(e => e.Zip)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("zip");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
