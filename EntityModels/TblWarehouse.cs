using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblWarehouse
{
    public int WarehouseId { get; set; }

    public string? Name { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
