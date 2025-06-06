namespace InventoryManagement.ViewModels
{
    public class StockCombinedViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BatchNo { get; set; }
        public string Quantity { get; set; }
        public string ProductName { get; set; }
        public string SKUName { get; set; }
        public string SupplierName { get; set; }
        public string LocationName { get; set; }
        public string Status { get; set; } 
        public string RoomName { get; set; } 
        public string RackName { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<string?> AliasNames { get; internal set; }
    }

    public class BatchDetailsViewModel
    {
        public string BatchNo { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public List<StockCombinedViewModel> StockItems { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
    }

}
