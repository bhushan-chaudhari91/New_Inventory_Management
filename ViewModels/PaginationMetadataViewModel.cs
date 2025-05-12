namespace InventoryManagement.ViewModels
{
    public class PaginationMetadataViewModel
    {
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public string SearchTerm { get; set; }
    }
}
