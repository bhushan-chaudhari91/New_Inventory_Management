using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModels
{
    public class StockOutViewModel
    {
        public int StockOutId { get; set; }

        public string Barcode { get; set; }

        public int FkProductId { get; set; }

        public string Quantity { get; set; }

        public string Reason { get; set; }

        public SelectList ProductList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    }



   
}
