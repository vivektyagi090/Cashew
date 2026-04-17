namespace WebApi.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQty { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? Brand { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed
        public int? DiscountPercent => OriginalPrice.HasValue && OriginalPrice > 0
            ? (int)Math.Round((1 - Price / OriginalPrice.Value) * 100)
            : null;
    }

    public class ProductListRequest
    {
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // price_asc, price_desc, rating, newest
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class ProductListResponse
    {
        public List<ProductModel> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
