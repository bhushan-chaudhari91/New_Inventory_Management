namespace InventoryManagement.ViewModels
{
    public class StockReportViewModel
    {
        public string ProductName { get; set; }
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("dd/MM/yyyy (hh:mm tt)").ToUpper();
        public string Type { get; set; }
        public string SKUName { get; set; }
        public int Quantity { get; set; }
        
    }

    public class StockReportPagedViewModel
    {
        public List<StockReportViewModel> CombinedStockList { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
    }

}
