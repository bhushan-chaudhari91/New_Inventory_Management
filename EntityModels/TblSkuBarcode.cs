using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblSkuBarcode
{
    public int SkuId { get; set; }

    public int? FkProductId { get; set; }

    public string? Skuname { get; set; }

    public int? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
