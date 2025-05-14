namespace VerticalSliceApi.Domain
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message)
            : base(message) { }
    }
}
