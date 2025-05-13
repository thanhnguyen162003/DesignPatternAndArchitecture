using FluentValidation;
using FluentValidation.Results;

using MediatR;

namespace CleanPatternWithCloudNative.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);
            if (!validators.Any())
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            ValidationFailure[] validationErrors = validators
                .Select(x => x.Validate(context))
                .Where(x => x.Errors.Count > 0)
                .SelectMany(x => x.Errors)
                .ToArray();

            if (validationErrors.Length > 0)
            {
                throw new ValidationException(validationErrors);
            }

            return await next(cancellationToken);
        }
    }
}