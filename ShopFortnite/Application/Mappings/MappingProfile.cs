using AutoMapper;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.UserCosmetics, opt => opt.MapFrom(src => src.UserCosmetics));
        CreateMap<User, UserWithCosmeticsDto>()
            .ForMember(dest => dest.Cosmetics, opt => opt.MapFrom(src => src.UserCosmetics));

        // Cosmetic mappings
        CreateMap<Cosmetic, CosmeticDto>();

        // UserCosmetic mappings
        CreateMap<UserCosmetic, UserCosmeticDto>()
            .ForMember(dest => dest.CosmeticId, opt => opt.MapFrom(src => src.CosmeticId))
            .ForMember(dest => dest.PurchasedAt, opt => opt.MapFrom(src => src.PurchaseDate));

        // Transaction mappings
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CosmeticName, opt => opt.MapFrom(src => src.Cosmetic.Name))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.BalanceAfter, opt => opt.Ignore());
    }
}
