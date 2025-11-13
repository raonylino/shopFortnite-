using AutoMapper;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Domain.Interfaces;

namespace ShopFortnite.Application.UseCases;

public interface ICosmeticService
{
    Task<PagedResult<CosmeticDto>> GetCosmeticsAsync(CosmeticQueryParameters parameters);
    Task<CosmeticDto?> GetCosmeticByIdAsync(Guid id);
}

public class CosmeticService : ICosmeticService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CosmeticService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<CosmeticDto>> GetCosmeticsAsync(CosmeticQueryParameters parameters)
    {
        var cosmetics = await _unitOfWork.Cosmetics.GetPagedAsync(
            parameters.Page,
            parameters.PageSize,
            parameters.Name,
            parameters.Type,
            parameters.Rarity,
            parameters.IsNew,
            parameters.IsForSale,
            parameters.FromDate
        );

        var totalCount = await _unitOfWork.Cosmetics.CountAsync(
            parameters.Name,
            parameters.Type,
            parameters.Rarity,
            parameters.IsNew,
            parameters.IsForSale,
            parameters.FromDate
        );

        return new PagedResult<CosmeticDto>
        {
            Items = _mapper.Map<IEnumerable<CosmeticDto>>(cosmetics),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<CosmeticDto?> GetCosmeticByIdAsync(Guid id)
    {
        var cosmetic = await _unitOfWork.Cosmetics.GetByIdAsync(id);
        return cosmetic == null ? null : _mapper.Map<CosmeticDto>(cosmetic);
    }
}
