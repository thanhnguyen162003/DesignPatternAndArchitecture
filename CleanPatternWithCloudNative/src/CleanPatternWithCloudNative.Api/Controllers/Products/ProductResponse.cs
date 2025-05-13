namespace CleanPatternWithCloudNative.Api.Controllers.Products
{
    /// <summary>
    ///     Product response
    /// </summary>
    public sealed record ProductResponse(
        Guid Id,
        string Name,
        string? Description);
}