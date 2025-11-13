using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Domain.Interfaces;

public interface IUserCosmeticRepository
{
    Task<UserCosmetic?> GetAsync(Guid userId, Guid cosmeticId);
    Task<IEnumerable<UserCosmetic>> GetByUserIdAsync(Guid userId);
    Task<bool> HasPurchasedAsync(Guid userId, Guid cosmeticId);
    Task<UserCosmetic> CreateAsync(UserCosmetic userCosmetic);
    Task UpdateAsync(UserCosmetic userCosmetic);
}
