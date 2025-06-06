using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblUsersRole
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public int? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }
}
