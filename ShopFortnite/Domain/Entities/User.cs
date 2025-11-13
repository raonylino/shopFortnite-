namespace ShopFortnite.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal Vbucks { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<UserCosmetic> UserCosmetics { get; set; } = new List<UserCosmetic>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Vbucks = 10000; // Initial credit
    }
}
