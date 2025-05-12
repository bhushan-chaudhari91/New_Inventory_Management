using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblStockIn
{
    public int StockInId { get; set; }
    public string? BatchNo { get; set; }

    public DateTime? Date { get; set; }

    public int? FkSupplierId { get; set; }

    public int? FkWarehouseId { get; set; }

    public int? FkProductId { get; set; }

    public string? Type { get; set; }

    public string? ProductQuantity { get; set; }

    public decimal? Price { get; set; }

    public string? AvailableQuantity { get; set; }

    public string? Room { get; set; }

    public string? RackNo { get; set; }

    public string? Barcode { get; set; }

    public string? ProductStatus { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
