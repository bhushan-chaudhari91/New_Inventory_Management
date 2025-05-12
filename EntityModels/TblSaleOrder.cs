namespace InventoryManagement.EntityModels
{
    public class TblSaleOrder
    {
        public int OrderId { get; set; }

        public string? OrderNumber { get; set; }

        public int? FkProductId { get; set; }

        public string? ProductName { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? OrderProductQty { get; set; }

        public string? Status { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
