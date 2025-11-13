namespace ShopFortnite.Domain.Entities;

public class UserCosmetic
{
    public Guid UserId { get; set; }
    public Guid CosmeticId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public decimal PriceAtPurchase { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Cosmetic Cosmetic { get; set; } = null!;

    public bool IsReturned => ReturnedDate.HasValue;
}
