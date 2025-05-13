using CleanPatternWithCloudNative.Application.Abstractions.Clock;
using CleanPatternWithCloudNative.Domain.Entities;
using CleanPatternWithCloudNative.Domain.Repositories;

using MediatR;

namespace CleanPatternWithCloudNative.Application.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler(
        IProductsRepository repository,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateProductCommand, Guid?>
    {
        public async Task<Guid?> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            Guid? productId = await repository.CreateProductAsync(
                new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    CreatedAtUtc = dateTimeProvider.Now
                },
                cancellationToken);

            return productId;
        }
    }
}