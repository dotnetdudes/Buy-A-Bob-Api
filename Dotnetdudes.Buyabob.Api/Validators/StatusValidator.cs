using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Buyabob.Api.Validators
{
    public class StatusValidator : AbstractValidator<Status>
    {
        public StatusValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
