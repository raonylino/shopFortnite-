using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CosmeticId { get; set; }
    public string CosmeticName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } // Alias para Date
}

public class PurchaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal RemainingVbucks { get; set; }
    public int NewBalance { get; set; } // Para compatibilidade com Razor Pages
    public TransactionDto? Transaction { get; set; }
}
