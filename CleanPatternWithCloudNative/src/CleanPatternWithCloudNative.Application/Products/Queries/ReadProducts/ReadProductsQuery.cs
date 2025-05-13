using CleanPatternWithCloudNative.Domain.Entities;

using MediatR;

namespace CleanPatternWithCloudNative.Application.Products.Queries.ReadProducts
{
    public sealed record ReadProductsQuery() : IRequest<Product[]>;
}