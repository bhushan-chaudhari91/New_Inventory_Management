using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModels
{
    public class StockInViewModel
    {
        public int StockInId { get; set; }

        public string BatchNo { get; set; }

        public DateTime Date { get; set; }

        public int FkSupplierId { get; set; }

        public int FkWarehouseId { get; set; }

        public int FkProductId { get; set; }
        public decimal Price { get; set; }

        public string Type { get; set; }
        public int IsBox { get; set; }

        public string ProductQuantity { get; set; }
        public string AvailableProductQty { get; set; }
        public string barcodeNo { get; set; }

        public string AvailableQuantity { get; set; }
        public string LowStockQty { get; set; }

        public string Room { get; set; }

        public string RackNo { get; set; }

        public string Barcode { get; set; }

        public string ProductStatus { get; set; }
        public decimal TotalBox { get; set; }
        public decimal PerBoxQty { get; set; }

      


        public string ProductName { get; set; }
        public string SKUName { get; set; }
        public string WarehouseName { get; set; }

        public SelectList SupplierList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList WarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList ProductList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList TypeList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public string Alias1 { get; set; }
        public string Alias2 { get; set; }
        public string Alias3 { get; set; }
        public List<string?> AliasNames { get; internal set; }
    }

    public class StockInListViewModel
    {
        public List<StockInViewModel> StockIns { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }
        public int UserFkRoleId { get; set; }
    }
}
