using MediatR;

namespace CleanPatternWithCloudNative.Application.Products.Commands.DeleteState
{
    public record DeleteStateCommand(string ProductName) : IRequest;
}