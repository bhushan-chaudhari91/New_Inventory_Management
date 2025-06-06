using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblProductAlias
{
    public int Id { get; set; }

    public int? FkProductId { get; set; }

    public string? AliasName { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
