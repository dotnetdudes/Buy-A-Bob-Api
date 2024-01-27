using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class ShippingAddressValidator : AbstractValidator<ShippingAddress>
    {
        public ShippingAddressValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.AddressId).NotEmpty();
        }
    }
}
