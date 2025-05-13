using MediatR;

namespace CleanPatternWithCloudNative.Application.Products.Commands.CreateProduct
{
    /// <summary>
    ///     Create product command
    /// </summary>
    /// <param name="Name">Product name</param>
    /// <param name="Description">Product description</param>
    public sealed record CreateProductCommand(string Name, string Description) : IRequest<Guid?>;
}