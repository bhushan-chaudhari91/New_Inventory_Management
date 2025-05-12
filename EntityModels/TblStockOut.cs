using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblStockOut
{
    public int StockOutId { get; set; }

    public string? Barcode { get; set; }

    public int? FkProductId { get; set; }
    public int? FkStockInId { get; set; }

    public string? Quantity { get; set; }

    public string? AvailableQuantity { get; set; }

    public string? Reason { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? StockOutDate { get; set; }
    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
