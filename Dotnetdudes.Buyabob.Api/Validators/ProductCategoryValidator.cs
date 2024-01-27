using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class ProductCategoryValidator : AbstractValidator<ProductCategory>
    {
        public ProductCategoryValidator()
        {
            RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId is required");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0");
            RuleFor(x => x.CategoryId).NotNull().WithMessage("CategoryId is required");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("CategoryId must be greater than 0");
        }
    }
}
