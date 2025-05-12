namespace InventoryManagement.ViewModels
{
    public class SaleOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int EkProjectId { get; set; }
        public string ProductName { get; set; }
        public string SKUName { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderQuantity { get; set; }
        public string AvailableQuantity { get; set; }
        public string Status { get; set; }
    }

    public class SaleOrderMasterListViewModel
    {
        public List<SaleOrderViewModel> SaleOrder { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
    }
}
