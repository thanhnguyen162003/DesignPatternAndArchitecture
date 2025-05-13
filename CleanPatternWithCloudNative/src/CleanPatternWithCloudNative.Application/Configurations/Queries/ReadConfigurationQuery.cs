using MediatR;

namespace CleanPatternWithCloudNative.Application.Configurations.Queries
{
    public record ReadConfigurationQuery(string Key) : IRequest<string?>;
}