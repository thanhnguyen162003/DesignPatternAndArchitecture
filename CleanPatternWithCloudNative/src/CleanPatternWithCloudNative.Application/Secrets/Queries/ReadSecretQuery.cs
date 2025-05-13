using MediatR;

namespace CleanPatternWithCloudNative.Application.Secrets.Queries
{
    public record ReadSecretQuery(string Key) : IRequest<string?>;
}