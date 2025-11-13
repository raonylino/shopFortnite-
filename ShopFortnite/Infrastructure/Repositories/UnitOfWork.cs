using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Infrastructure.Data;

namespace ShopFortnite.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private ICosmeticRepository? _cosmetics;
    private IUserCosmeticRepository? _userCosmetics;
    private ITransactionRepository? _transactions;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICosmeticRepository Cosmetics => _cosmetics ??= new CosmeticRepository(_context);
    public IUserCosmeticRepository UserCosmetics => _userCosmetics ??= new UserCosmeticRepository(_context);
    public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
