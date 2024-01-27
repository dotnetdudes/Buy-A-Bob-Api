using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
            RuleFor(x => x.Price).NotEmpty().WithMessage("Price is required");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
            RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("ImageUrl is required");
            RuleFor(x => x.ImageUrl).MaximumLength(255).WithMessage("ImageUrl must not exceed 255 characters.");
            RuleFor(x => x.ImageUrl).Matches(@"\.(gif|jpg|jpeg|png)$").WithMessage("ImageUrl must be a valid image file.");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            RuleFor(x => x.Weight).NotEmpty().WithMessage("Weight is required");
            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be greater than 0");
            RuleFor(x => x.Width).NotEmpty().WithMessage("Width is required");
            RuleFor(x => x.Width).GreaterThan(0).WithMessage("Width must be greater than 0");
            RuleFor(x => x.Depth).NotEmpty().WithMessage("Depth is required");
            RuleFor(x => x.Depth).GreaterThan(0).WithMessage("Depth must be greater than 0");
            RuleFor(x => x.Height).NotEmpty().WithMessage("Height is required");
            RuleFor(x => x.Height).GreaterThan(0).WithMessage("Height must be greater than 0");
        }
    }   
}
