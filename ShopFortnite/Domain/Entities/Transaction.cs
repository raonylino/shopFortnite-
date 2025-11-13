namespace ShopFortnite.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CosmeticId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Cosmetic Cosmetic { get; set; } = null!;

    public Transaction()
    {
        Id = Guid.NewGuid();
        Date = DateTime.UtcNow;
    }
}

public enum TransactionType
{
    Purchase,
    Return
}
