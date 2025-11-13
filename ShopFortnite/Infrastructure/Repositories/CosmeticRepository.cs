using Microsoft.EntityFrameworkCore;
using ShopFortnite.Domain.Entities;
using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Infrastructure.Data;

namespace ShopFortnite.Infrastructure.Repositories;

public class CosmeticRepository : ICosmeticRepository
{
    private readonly AppDbContext _context;

    public CosmeticRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cosmetic?> GetByIdAsync(Guid id)
    {
        return await _context.Cosmetics.FindAsync(id);
    }

    public async Task<Cosmetic?> GetByExternalIdAsync(string externalId)
    {
        return await _context.Cosmetics.FirstOrDefaultAsync(c => c.ExternalId == externalId);
    }

    public async Task<IEnumerable<Cosmetic>> GetAllAsync()
    {
        return await _context.Cosmetics.ToListAsync();
    }

    public async Task<IEnumerable<Cosmetic>> GetPagedAsync(int page, int pageSize, string? name = null,
        string? type = null, string? rarity = null, bool? isNew = null,
        bool? isForSale = null, DateTime? fromDate = null)
    {
        var query = _context.Cosmetics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(c => c.Type == type);

        if (!string.IsNullOrWhiteSpace(rarity))
            query = query.Where(c => c.Rarity == rarity);

        if (isNew.HasValue)
            query = query.Where(c => c.IsNew == isNew.Value);

        if (isForSale.HasValue)
            query = query.Where(c => c.IsForSale == isForSale.Value);

        if (fromDate.HasValue)
            query = query.Where(c => c.AddedDate >= fromDate.Value);

        return await query
            .OrderByDescending(c => c.AddedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? name = null, string? type = null, string? rarity = null,
        bool? isNew = null, bool? isForSale = null, DateTime? fromDate = null)
    {
        var query = _context.Cosmetics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(c => c.Type == type);

        if (!string.IsNullOrWhiteSpace(rarity))
            query = query.Where(c => c.Rarity == rarity);

        if (isNew.HasValue)
            query = query.Where(c => c.IsNew == isNew.Value);

        if (isForSale.HasValue)
            query = query.Where(c => c.IsForSale == isForSale.Value);

        if (fromDate.HasValue)
            query = query.Where(c => c.AddedDate >= fromDate.Value);

        return await query.CountAsync();
    }

    public async Task<Cosmetic> CreateAsync(Cosmetic cosmetic)
    {
        await _context.Cosmetics.AddAsync(cosmetic);
        return cosmetic;
    }

    public Task UpdateAsync(Cosmetic cosmetic)
    {
        _context.Cosmetics.Update(cosmetic);
        return Task.CompletedTask;
    }

    public async Task CreateOrUpdateManyAsync(IEnumerable<Cosmetic> cosmetics)
    {
        // Agrupa por ExternalId para evitar duplicatas no mesmo batch
        var uniqueCosmetics = cosmetics
            .GroupBy(c => c.ExternalId)
            .Select(g => g.First())
            .ToList();

        // Busca todos os ExternalIds existentes de uma vez
        var externalIds = uniqueCosmetics.Select(c => c.ExternalId).ToList();
        var existingCosmetics = await _context.Cosmetics
            .Where(c => externalIds.Contains(c.ExternalId))
            .ToListAsync();

        var existingDict = existingCosmetics.ToDictionary(c => c.ExternalId);

        foreach (var cosmetic in uniqueCosmetics)
        {
            if (existingDict.TryGetValue(cosmetic.ExternalId, out var existing))
            {
                existing.Name = cosmetic.Name;
                existing.Type = cosmetic.Type;
                existing.Rarity = cosmetic.Rarity;
                existing.Price = cosmetic.Price;
                existing.ImageUrl = cosmetic.ImageUrl;
                existing.IsNew = cosmetic.IsNew;
                existing.IsForSale = cosmetic.IsForSale;
                existing.Description = cosmetic.Description;
                _context.Cosmetics.Update(existing);
            }
            else
            {
                // Garante que cada cosmético tenha um novo ID único
                cosmetic.Id = Guid.NewGuid();
                await _context.Cosmetics.AddAsync(cosmetic);
            }
        }
    }
}
