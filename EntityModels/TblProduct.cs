using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblProduct
{
    public int ProductId { get; set; }
    public int FK_Unit { get; set; }
    public string? ProductName { get; set; }
    public string? SkuIdName { get; set; }
    public string? AvailableProductQty { get; set; }
    public string? LowStockQuantity { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
