using MediatR;

namespace CleanPatternWithCloudNative.Application.States.Queries.ReadState
{
    public record ReadStateQuery(string ProductName) : IRequest<ProductState?>;
}