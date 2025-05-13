using FluentValidation;

namespace CleanPatternWithCloudNative.Application.States.Commands.SaveState
{
    public class SaveStateCommandValidator : AbstractValidator<SaveStateCommand>
    {
        public SaveStateCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Description)
                .MaximumLength(200);
        }
    }
}