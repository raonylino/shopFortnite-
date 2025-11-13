using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
}
