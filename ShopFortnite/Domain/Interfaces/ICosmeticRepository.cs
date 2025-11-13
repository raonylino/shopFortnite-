using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Domain.Interfaces;

public interface ICosmeticRepository
{
    Task<Cosmetic?> GetByIdAsync(Guid id);
    Task<Cosmetic?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Cosmetic>> GetAllAsync();
    Task<IEnumerable<Cosmetic>> GetPagedAsync(int page, int pageSize, string? name = null,
        string? type = null, string? rarity = null, bool? isNew = null,
        bool? isForSale = null, DateTime? fromDate = null);
    Task<int> CountAsync(string? name = null, string? type = null, string? rarity = null,
        bool? isNew = null, bool? isForSale = null, DateTime? fromDate = null);
    Task<Cosmetic> CreateAsync(Cosmetic cosmetic);
    Task UpdateAsync(Cosmetic cosmetic);
    Task CreateOrUpdateManyAsync(IEnumerable<Cosmetic> cosmetics);
}
