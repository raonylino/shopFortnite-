using Microsoft.EntityFrameworkCore;
using ShopFortnite.Domain.Entities;
using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Infrastructure.Data;

namespace ShopFortnite.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Transactions
            .Include(t => t.Cosmetic)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }
}
