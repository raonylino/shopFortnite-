namespace ShopFortnite.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICosmeticRepository Cosmetics { get; }
    IUserCosmeticRepository UserCosmetics { get; }
    ITransactionRepository Transactions { get; }
    Task<int> SaveChangesAsync();
}
