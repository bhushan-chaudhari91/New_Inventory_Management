namespace InventoryManagement.ViewModels
{
    public class BatchSummaryViewModel
    {
        public int StockInId { get; set; }
        public string BatchNo { get; set; }
        public DateTime? Date { get; set; }
        public int ProductsCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public int WarehouseCount { get; set; }
    }

    public class BatchListViewModel
    {
        public List<BatchSummaryViewModel> Batches { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
    }
}
