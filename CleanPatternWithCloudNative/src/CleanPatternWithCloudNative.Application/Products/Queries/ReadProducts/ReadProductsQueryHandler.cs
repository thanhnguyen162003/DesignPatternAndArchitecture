using CleanPatternWithCloudNative.Application.Abstractions.Caching;
using CleanPatternWithCloudNative.Domain.Abstract;
using CleanPatternWithCloudNative.Domain.Entities;
using CleanPatternWithCloudNative.Domain.Repositories;

using MediatR;

using Microsoft.Extensions.Logging;

namespace CleanPatternWithCloudNative.Application.Products.Queries.ReadProducts
{
    public sealed class ReadProductsQueryHandler(
        IProductsRepository repository,
        ICacheService cacheService,
        ILogger<ReadProductsQueryHandler> logger) : IRequestHandler<ReadProductsQuery, Product[]>
    {
        public async Task<Product[]> Handle(ReadProductsQuery request, CancellationToken cancellationToken)
        {
            Product[]? cachedProducts = await cacheService.GetAsync<Product[]>(Constants.AllProductsCacheKey, cancellationToken);

            if (cachedProducts is null)
            {
                cachedProducts = (await repository.GetProductsAsync(cancellationToken)).ToArray();

                await cacheService.SetAsync(
                    Constants.AllProductsCacheKey,
                    cachedProducts!,
                    null,
                    cancellationToken);
            }

            logger.LogInformation("Querying products: {@Products}", cachedProducts);

            return cachedProducts;
        }
    }
}