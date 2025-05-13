namespace CleanPatternWithCloudNative.Api.Controllers.Products
{
    /// <summary>
    ///     Request to create product
    /// </summary>
    public sealed record CreateProductRequest(
        string Name,
        string Description);
}