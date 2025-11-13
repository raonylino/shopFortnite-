namespace ShopFortnite.Application.DTOs;

public class CosmeticDto
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsNew { get; set; }
    public bool IsForSale { get; set; }
    public DateTime AddedDate { get; set; }
    public string? Description { get; set; }
}

public class CosmeticQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Rarity { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsForSale { get; set; }
    public DateTime? FromDate { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
