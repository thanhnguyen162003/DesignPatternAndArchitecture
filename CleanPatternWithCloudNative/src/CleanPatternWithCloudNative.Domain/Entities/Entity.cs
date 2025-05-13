namespace CleanPatternWithCloudNative.Domain.Entities
{
    public abstract class Entity
    {
        protected Entity(Guid id) => Id = id;

        protected Entity()
        {
        }

        /// <summary>
        ///     Unique Identifier
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        ///     Created at time in UTC
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        ///     Updated at time in UTC
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }
    }
}