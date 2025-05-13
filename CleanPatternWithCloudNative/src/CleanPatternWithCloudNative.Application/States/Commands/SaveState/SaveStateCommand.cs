using MediatR;

namespace CleanPatternWithCloudNative.Application.States.Commands.SaveState
{
    public record SaveStateCommand(
        string Name,
        string Description) : IRequest;
}