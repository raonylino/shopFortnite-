namespace ShopFortnite.Domain.Entities;

public class Cosmetic
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

    // Navigation properties
    public ICollection<UserCosmetic> UserCosmetics { get; set; } = new List<UserCosmetic>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public Cosmetic()
    {
        Id = Guid.NewGuid();
        AddedDate = DateTime.UtcNow;
    }
}
