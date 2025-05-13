using System.Collections.ObjectModel;

namespace CleanPatternWithCloudNative.Api.Controllers.Products
{
    /// <summary>
    ///     Read products - response
    /// </summary>
    public record ReadProductsResponse(Collection<ProductResponse> Products);
}