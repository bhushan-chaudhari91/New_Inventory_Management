using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace InventoryManagement.EntityModels;

public partial class DbInventoryContext : DbContext
{
    public DbInventoryContext()
    {
    }

    public DbInventoryContext(DbContextOptions<DbInventoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblBarcode> TblBarcodes { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblProductAlias> TblProductAliases { get; set; }

    public virtual DbSet<TblStockIn> TblStockIns { get; set; }

    public virtual DbSet<TblStockOut> TblStockOuts { get; set; }

    public virtual DbSet<TblSupplier> TblSuppliers { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblWarehouse> TblWarehouses { get; set; }

    public virtual DbSet<TblSaleOrder> TblSaleOrders { get; set; }

    public virtual DbSet<TblUsersRole> TblUsersRoles { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<TblBarcode>(entity =>
        {
            entity.HasKey(e => e.BarcodeId).HasName("PRIMARY");

            entity.ToTable("tbl_barcode");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FkProductId).HasColumnName("Fk_ProductId");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("tbl_product");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FK_Unit).HasColumnName("FK_Unit");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.ProductName).HasMaxLength(50);
            entity.Property(e => e.SkuIdName)
                .HasMaxLength(45)
                .HasColumnName("SKU_Id_Name");
            entity.Property(e => e.AvailableProductQty)
              .HasMaxLength(45)
              .HasColumnName("AvailableProductQty");
            entity.Property(e => e.LowStockQuantity)
               .HasMaxLength(45)
               .HasColumnName("LowStockQuantity");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblProductAlias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tbl_product_alias");

            entity.Property(e => e.AliasName).HasMaxLength(45);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FkProductId).HasColumnName("Fk_ProductId");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblStockIn>(entity =>
        {
            entity.HasKey(e => e.StockInId).HasName("PRIMARY");

            entity.ToTable("tbl_stock_in");

            entity.Property(e => e.AvailableQuantity).HasMaxLength(45);
            entity.Property(e => e.Barcode).HasMaxLength(45);
            entity.Property(e => e.BatchNo).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.FkProductId).HasColumnName("Fk_ProductId");
            entity.Property(e => e.FkSupplierId).HasColumnName("Fk_SupplierId");
            entity.Property(e => e.FkWarehouseId).HasColumnName("Fk_WarehouseId");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.Price).HasPrecision(10);
            entity.Property(e => e.ProductQuantity).HasMaxLength(45);
            entity.Property(e => e.ProductStatus).HasMaxLength(45);
            entity.Property(e => e.RackNo).HasMaxLength(45);
            entity.Property(e => e.Room).HasMaxLength(45);
            entity.Property(e => e.Type).HasMaxLength(45);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblStockOut>(entity =>
        {
            entity.HasKey(e => e.StockOutId).HasName("PRIMARY");

            entity.ToTable("tbl_stock_out");

            entity.Property(e => e.AvailableQuantity).HasMaxLength(45);
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.StockOutDate)
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .HasColumnType("datetime");
            entity.Property(e => e.FkProductId).HasColumnName("Fk_ProductId");
            entity.Property(e => e.FkStockInId).HasColumnName("FkStockInId");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.Quantity).HasMaxLength(45);
            entity.Property(e => e.Reason).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });



        modelBuilder.Entity<TblSupplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PRIMARY");

            entity.ToTable("tbl_supplier");

            entity.Property(e => e.ContactNo).HasMaxLength(45);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(45);
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.SupplierName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("tbl_user");

            entity.Property(e => e.ContactNo).HasMaxLength(45);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FkRoleId).HasColumnName("Fk_RoleId");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(45);
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<TblWarehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PRIMARY");

            entity.ToTable("tbl_warehouse");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblSaleOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("tbl_sale_orders");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FkProductId).HasColumnName("Fk_ProductId");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(45);
            entity.Property(e => e.OrderProductQty).HasMaxLength(45);
            entity.Property(e => e.ProductName).HasMaxLength(45);
            entity.Property(e => e.Status).HasMaxLength(45);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblUsersRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("tbl_users_role");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
            entity.Property(e => e.RoleName).HasMaxLength(45);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
