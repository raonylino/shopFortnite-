using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Application.UseCases;
using ShopFortnite.Domain.Interfaces;
using AutoMapper;

namespace ShopFortnite.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UsersController(IUserService userService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWithCosmeticsDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var usersWithCosmetics = new List<UserWithCosmeticsDto>();

        foreach (var user in users)
        {
            var userWithCosmetics = await _userService.GetUserWithCosmeticsAsync(user.Id);
            if (userWithCosmetics != null)
            {
                usersWithCosmetics.Add(userWithCosmetics);
            }
        }

        return Ok(usersWithCosmetics);
    }

    [HttpGet("me")]
    [Authorize] // Garante que só usuários autenticados podem acessar
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        // Pega o ID do usuário do token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(new { message = "Token inválido ou ausente" });
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new { message = "ID do usuário inválido no token" });
        }

        var userWithCosmetics = await _userService.GetUserWithCosmeticsAsync(userId);

        if (userWithCosmetics == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        // Formata a resposta no formato solicitado
        var response = new CurrentUserDto
        {
            User = new UserInfoDto
            {
                Id = userWithCosmetics.Id,
                Email = userWithCosmetics.Email,
                Name = userWithCosmetics.Name,
                Vbucks = userWithCosmetics.Vbucks,
                CreatedAt = userWithCosmetics.CreatedAt
            },
            Cosmetics = userWithCosmetics.Cosmetics
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserWithCosmeticsDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserWithCosmeticsAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }
        return Ok(user);
    }

    [HttpGet("{id}/cosmetics")]
    public async Task<ActionResult<List<UserCosmeticDto>>> GetUserCosmetics(Guid id)
    {
        var user = await _userService.GetUserWithCosmeticsAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }
        return Ok(user.Cosmetics);
    }

    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<List<TransactionDto>>> GetUserTransactions(Guid id)
    {
        var transactions = await _unitOfWork.Transactions.GetByUserIdAsync(id);
        var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
        return Ok(transactionDtos);
    }
}
