namespace CleanPatternWithCloudNative.Domain.Entities
{
    /// <summary>
    ///     Product entity
    /// </summary>
    public sealed class Product : Entity
    {
        /// <summary>
        ///     Product name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        ///     Product description
        /// </summary>
        public string? Description { get; set; }
    }
}