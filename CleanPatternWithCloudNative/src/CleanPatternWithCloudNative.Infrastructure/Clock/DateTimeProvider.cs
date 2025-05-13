using CleanPatternWithCloudNative.Application.Abstractions.Clock;

namespace CleanPatternWithCloudNative.Infrastructure.Clock
{
    // <see cref="IDateTimeProvider"/>
    public sealed class DateTimeProvider : IDateTimeProvider
    {
        // <see cref="IDateTimeProvider"/>
        public DateTime Now => DateTime.UtcNow;
    }
}