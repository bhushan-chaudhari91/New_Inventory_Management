using InventoryManagement.ViewModels;

namespace InventoryManagement.ViewModels
{
    public class WarehouseViewModel
    {
        public int WarehouseId { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int UpdatedBy { get; set; }
    }

    public class WarehouseListViewModel
    {
        public List<WarehouseViewModel> Warehouses { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
    }
}
