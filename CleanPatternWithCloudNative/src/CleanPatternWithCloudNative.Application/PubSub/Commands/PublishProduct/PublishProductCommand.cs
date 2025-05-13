using MediatR;

namespace CleanPatternWithCloudNative.Application.PubSub.Commands.PublishProduct
{
    public record PublishProductCommand(string Name, string Description) : IRequest;
}