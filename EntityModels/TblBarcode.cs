using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblBarcode
{
    public int BarcodeId { get; set; }

    public int? FkProductId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
