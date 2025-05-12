namespace InventoryManagement.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SkuIdName { get; set; }
        public string LowStockQuantity { get; set; }
        public string unit { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }

        public int FkProdictId { get; set; }
        public string GetProductName { get; set; }
        public string AliasName { get; set; }
        public List<string> AliasNames { get; set; }
        public List<ProductDto> ProductNames { get; set; }

    }

    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; }
        public PaginationMetadataViewModel Pagination { get; set; }

        public ProductViewModel EditProduct { get; set; }

    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

}
