using System;
using System.Collections.Generic;

namespace InventoryManagement.EntityModels;

public partial class TblUser
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? ContactNo { get; set; }

    public string? Address { get; set; }

    public int? FkRoleId { get; set; }

    public string? Gender { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
