namespace InventoryManagement.ViewModels
{
    public class SupplierViewModel
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class SupplierListViewModel
    {
        public List<SupplierViewModel> Supplies { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
    }
}
