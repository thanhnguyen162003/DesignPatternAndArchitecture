namespace CleanPatternWithCloudNative.Application.Abstractions.Clock
{
    /// <summary>
    ///     Date time provider
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        ///     Easy to mock
        /// </summary>
        DateTime Now { get; }
    }
}