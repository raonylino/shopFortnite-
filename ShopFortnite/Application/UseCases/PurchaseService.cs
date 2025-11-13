using AutoMapper;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Domain.Entities;
using ShopFortnite.Domain.Interfaces;

namespace ShopFortnite.Application.UseCases;

public interface IPurchaseService
{
    Task<PurchaseResponse> PurchaseCosmeticAsync(Guid userId, Guid cosmeticId);
    Task<PurchaseResponse> ReturnCosmeticAsync(Guid userId, Guid cosmeticId);
}

public class PurchaseService : IPurchaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PurchaseResponse> PurchaseCosmeticAsync(Guid userId, Guid cosmeticId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return new PurchaseResponse { Success = false, Message = "Usuário não encontrado" };
        }

        var cosmetic = await _unitOfWork.Cosmetics.GetByIdAsync(cosmeticId);
        if (cosmetic == null)
        {
            return new PurchaseResponse { Success = false, Message = "Cosmético não encontrado" };
        }

        if (!cosmetic.IsForSale)
        {
            return new PurchaseResponse { Success = false, Message = "Cosmético não está à venda" };
        }

        // Check if user already purchased this cosmetic
        var existingPurchase = await _unitOfWork.UserCosmetics.GetAsync(userId, cosmeticId);
        if (existingPurchase != null && !existingPurchase.IsReturned)
        {
            return new PurchaseResponse { Success = false, Message = "Você já possui este cosmético" };
        }

        // Check if user has enough vbucks
        if (user.Vbucks < cosmetic.Price)
        {
            return new PurchaseResponse
            {
                Success = false,
                Message = $"V-bucks insuficientes. Necessário: {cosmetic.Price}, Disponível: {user.Vbucks}"
            };
        }

        // Debit vbucks
        user.Vbucks -= cosmetic.Price;

        // Create or update UserCosmetic
        if (existingPurchase != null)
        {
            existingPurchase.PurchaseDate = DateTime.UtcNow;
            existingPurchase.ReturnedDate = null;
            existingPurchase.PriceAtPurchase = cosmetic.Price;
            await _unitOfWork.UserCosmetics.UpdateAsync(existingPurchase);
        }
        else
        {
            var userCosmetic = new UserCosmetic
            {
                UserId = userId,
                CosmeticId = cosmeticId,
                PurchaseDate = DateTime.UtcNow,
                PriceAtPurchase = cosmetic.Price
            };
            await _unitOfWork.UserCosmetics.CreateAsync(userCosmetic);
        }

        // Create transaction
        var transaction = new Transaction
        {
            UserId = userId,
            CosmeticId = cosmeticId,
            Type = TransactionType.Purchase,
            Amount = cosmetic.Price
        };
        await _unitOfWork.Transactions.CreateAsync(transaction);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new PurchaseResponse
        {
            Success = true,
            Message = "Compra realizada com sucesso",
            RemainingVbucks = user.Vbucks,
            NewBalance = (int)user.Vbucks,
            Transaction = _mapper.Map<TransactionDto>(transaction)
        };
    }

    public async Task<PurchaseResponse> ReturnCosmeticAsync(Guid userId, Guid cosmeticId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return new PurchaseResponse { Success = false, Message = "Usuário não encontrado" };
        }

        var userCosmetic = await _unitOfWork.UserCosmetics.GetAsync(userId, cosmeticId);
        if (userCosmetic == null || userCosmetic.IsReturned)
        {
            return new PurchaseResponse { Success = false, Message = "Você não possui este cosmético ou já foi devolvido" };
        }

        // Refund vbucks
        user.Vbucks += userCosmetic.PriceAtPurchase;
        userCosmetic.ReturnedDate = DateTime.UtcNow;

        // Create transaction
        var transaction = new Transaction
        {
            UserId = userId,
            CosmeticId = cosmeticId,
            Type = TransactionType.Return,
            Amount = userCosmetic.PriceAtPurchase
        };
        await _unitOfWork.Transactions.CreateAsync(transaction);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.UserCosmetics.UpdateAsync(userCosmetic);
        await _unitOfWork.SaveChangesAsync();

        return new PurchaseResponse
        {
            Success = true,
            Message = "Devolução realizada com sucesso",
            RemainingVbucks = user.Vbucks,
            NewBalance = (int)user.Vbucks,
            Transaction = _mapper.Map<TransactionDto>(transaction)
        };
    }
}
