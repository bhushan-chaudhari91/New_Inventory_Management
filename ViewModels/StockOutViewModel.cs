using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModels
{
    public class StockOutViewModel
    {
        public int StockOutId { get; set; }

        public string Barcode { get; set; }
        public string SKUName { get; set; }
        public string RackNo { get; set; }

        public int FkProductId { get; set; }
        public int FkWarehouseId { get; set; }

        public string Quantity { get; set; }

        public string Reason { get; set; }

        public SelectList ProductList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList WarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    }



   
}
