using FluentValidation;

namespace CleanPatternWithCloudNative.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Description)
                .MaximumLength(200);
        }
    }
}