using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class CartValidator : AbstractValidator<Cart>
    {
        public CartValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.StatusId).NotEmpty();
        }
    }
}
