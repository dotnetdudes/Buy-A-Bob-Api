using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(x => x.CartId).NotEmpty().WithMessage("CartId is required");
            RuleFor(x => x.CartId).GreaterThan(0).WithMessage("CartId must be greater than 0");
            RuleFor(x => x.StatusId).NotEmpty().WithMessage("StatusId is required");
            RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("StatusId must be greater than 0");
            RuleFor(x => x.SubTotal).NotEmpty().WithMessage("SubTotal is required");
            RuleFor(x => x.SubTotal).GreaterThan(0).WithMessage("SubTotal must be greater than 0");
            RuleFor(x => x.Tax).NotEmpty().WithMessage("Tax is required");
            RuleFor(x => x.ShippingTypeId).NotEmpty().WithMessage("ShippingTypeId is required");
            RuleFor(x => x.ShippingTypeId).GreaterThan(0).WithMessage("ShippingTypeId must be greater than 0");
            RuleFor(x => x.Shipping).NotEmpty().WithMessage("Shipping is required");
            RuleFor(x => x.ShippingAddressId).NotEmpty().WithMessage("ShippingAddressId is required");
            RuleFor(x => x.ShippingAddressId).GreaterThan(0).WithMessage("ShippingAddressId must be greater than 0");
            RuleFor(x => x.Total).NotEmpty().WithMessage("Total is required");
            RuleFor(x => x.Total).GreaterThan(0).WithMessage("Total must be greater than 0");
        }
    }
}
