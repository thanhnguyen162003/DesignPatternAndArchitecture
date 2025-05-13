using CleanPatternWithCloudNative.Domain.Entities;

namespace CleanPatternWithCloudNative.Domain.Repositories
{
    /// <summary>
    ///     Products repository
    /// </summary>
    public interface IProductsRepository
    {
        /// <summary>
        ///     Create product
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Product id</returns>
        Task<Guid?> CreateProductAsync(Product product, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Get all products
        /// </summary>
        /// <returns>Products</returns>
        Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
    }
}