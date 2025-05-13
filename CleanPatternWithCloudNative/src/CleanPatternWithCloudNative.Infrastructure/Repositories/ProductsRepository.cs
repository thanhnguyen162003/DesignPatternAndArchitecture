using CleanPatternWithCloudNative.Domain.Entities;
using CleanPatternWithCloudNative.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace CleanPatternWithCloudNative.Infrastructure.Repositories
{
    // <see cref="IProductsRepository"/>
    public sealed class ProductsRepository(ApplicationDbContext dbContext) : IProductsRepository
    {
        // <see cref="IProductsRepository"/>
        public async Task<Guid?> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            await dbContext
                .AddAsync(product, cancellationToken);

            await dbContext
                .SaveChangesAsync(cancellationToken);

            return product?.Id;
        }

        // <see cref="IProductsRepository"/>
        public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            Product[] products = await dbContext
                .Set<Product>()
                .ToArrayAsync(cancellationToken);

            return products;
        }
    }
}