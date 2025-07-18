using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblRack
{
    public int RackId { get; set; }

    public int? FkWarehouseId { get; set; }

    public string? RackNo { get; set; }

    public int? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
