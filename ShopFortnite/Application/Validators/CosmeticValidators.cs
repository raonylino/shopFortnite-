using FluentValidation;
using ShopFortnite.Application.DTOs;

namespace ShopFortnite.Application.Validators;

public class CosmeticQueryParametersValidator : AbstractValidator<CosmeticQueryParameters>
{
    public CosmeticQueryParametersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page deve ser maior que 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize deve ser maior que 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize não pode ser maior que 100");
    }
}
