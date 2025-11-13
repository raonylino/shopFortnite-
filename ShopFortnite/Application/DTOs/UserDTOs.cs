namespace ShopFortnite.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Vbucks { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<UserCosmeticDto> UserCosmetics { get; set; } = new List<UserCosmeticDto>();
}

public class UserWithCosmeticsDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Vbucks { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<UserCosmeticDto> Cosmetics { get; set; } = new List<UserCosmeticDto>();
}

public class CurrentUserDto
{
    public UserInfoDto User { get; set; } = null!;
    public IEnumerable<UserCosmeticDto> Cosmetics { get; set; } = new List<UserCosmeticDto>();
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Vbucks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserCosmeticDto
{
    public Guid CosmeticId { get; set; }
    public CosmeticDto Cosmetic { get; set; } = null!;
    public DateTime PurchaseDate { get; set; }
    public DateTime PurchasedAt { get; set; } // Alias para PurchaseDate
    public decimal PriceAtPurchase { get; set; }
    public bool IsReturned { get; set; }
    public DateTime? ReturnedDate { get; set; }
}
