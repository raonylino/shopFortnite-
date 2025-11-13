using Microsoft.EntityFrameworkCore;
using ShopFortnite.Domain.Entities;
using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Infrastructure.Data;

namespace ShopFortnite.Infrastructure.Repositories;

public class UserCosmeticRepository : IUserCosmeticRepository
{
    private readonly AppDbContext _context;

    public UserCosmeticRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserCosmetic?> GetAsync(Guid userId, Guid cosmeticId)
    {
        return await _context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);
    }

    public async Task<IEnumerable<UserCosmetic>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .Where(uc => uc.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> HasPurchasedAsync(Guid userId, Guid cosmeticId)
    {
        return await _context.UserCosmetics
            .AnyAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId && !uc.ReturnedDate.HasValue);
    }

    public async Task<UserCosmetic> CreateAsync(UserCosmetic userCosmetic)
    {
        await _context.UserCosmetics.AddAsync(userCosmetic);
        return userCosmetic;
    }

    public Task UpdateAsync(UserCosmetic userCosmetic)
    {
        _context.UserCosmetics.Update(userCosmetic);
        return Task.CompletedTask;
    }
}
