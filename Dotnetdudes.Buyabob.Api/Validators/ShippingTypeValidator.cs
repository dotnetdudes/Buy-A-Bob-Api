using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class ShippingTypeValidator : AbstractValidator<ShippingType>
    {
        public ShippingTypeValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
