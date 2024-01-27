using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class CartStatusValidator : AbstractValidator<CartStatus>
    {
        public CartStatusValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
