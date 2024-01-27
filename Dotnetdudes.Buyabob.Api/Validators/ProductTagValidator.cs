using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class ProductTagValidator : AbstractValidator<ProductTag>
    {
        public ProductTagValidator()
        {
            RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId is required");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0");
            RuleFor(x => x.TagId).NotNull().WithMessage("TagId is required");
            RuleFor(x => x.TagId).GreaterThan(0).WithMessage("TagId must be greater than 0");
        }
    }
}
