namespace InventoryManagement.ViewModels
{
    //public class StockDetailsViewModel
    //{
    //    public List<StockInPageViewModel> StockInList { get; set; }
    //    public List<StockOutPageViewModel> StockOutList { get; set; }

    //}

    //public class StockInPageViewModel
    //{
    //    public int StockInId { get; set; }
    //    public DateTime StcokInDate { get; set; }
    //    public string ProductName { get; set; }
    //    public int BatchId { get; set; }
    //    public string SupplierName { get; set; }
    //    public string StockInQuantity { get; set; }
    //}

    //public class StockOutPageViewModel
    //{
    //    public int StcokOutId { get; set; }
    //    public DateTime StcokOutDate { get; set; }
    //    public string SKUtName { get; set; }
    //    public string StockOutQuantity { get; set; }
    //    public string Reason { get; set; }
    //}




    //public class StockDetailsViewModel
    //{
    //    public int StockInId { get; set; }
    //    public DateTime Date { get; set; }
    //    public int FkProductId { get; set; }
    //    public string ProductName { get; set; }
    //    public int FkSupplierId { get; set; }
    //    public string SupplierName { get; set; }
    //    public int BatchId { get; set; }
    //    public string ProductQuantity { get; set; }
    //    public int FkWarehouseId { get; set; }
    //}


    public class StockDetailsViewModel
    {
        public List<StockCombinedViewModel> CombinedStockList { get; set; }

        public PaginationMetadataViewModel Pagination { get; set; }

        public string ProductName { get; set; }
        public string SKUName { get; set; }
        public string AliasNames { get; set; }
        public int StockInId { get; set; }
        public int FkProjectId { get; set; }
        public string ProductQty { get; set; }



        public List<string> AliasNameList { get; set; }

    }

}
