using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModels
{
    public class RackNoVeiwModel
    {
        public int RackId { get; set; }
        public int FKWarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string RackNo { get; set; }
        public SelectList WarehouseList { get; internal set; }
    }
    public class RackNoListViewModel
    {
        public List<RackNoVeiwModel> rackNo { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
        //public SelectList WarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList WarehouseList { get; internal set; }
        public int FKWarehouseId { get; set; }



        //public IEnumerable<SelectListItem> WarehouseList { get; set; }
    }
}
