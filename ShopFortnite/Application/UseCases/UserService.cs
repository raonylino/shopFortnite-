using AutoMapper;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Domain.Interfaces;

namespace ShopFortnite.Application.UseCases;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserWithCosmeticsDto?> GetUserWithCosmeticsAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserWithCosmeticsDto?> GetUserWithCosmeticsAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return null;

        var userCosmetics = await _unitOfWork.UserCosmetics.GetByUserIdAsync(id);

        var userDto = _mapper.Map<UserWithCosmeticsDto>(user);
        userDto.Cosmetics = _mapper.Map<IEnumerable<UserCosmeticDto>>(
            userCosmetics.Where(uc => !uc.IsReturned)
        );

        return userDto;
    }
}
