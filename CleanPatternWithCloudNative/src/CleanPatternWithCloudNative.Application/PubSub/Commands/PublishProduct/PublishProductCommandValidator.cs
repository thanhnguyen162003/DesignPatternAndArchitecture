using FluentValidation;

namespace CleanPatternWithCloudNative.Application.PubSub.Commands.PublishProduct
{
    public class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
    {
        public PublishProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Description)
                .MaximumLength(200);
        }
    }
}