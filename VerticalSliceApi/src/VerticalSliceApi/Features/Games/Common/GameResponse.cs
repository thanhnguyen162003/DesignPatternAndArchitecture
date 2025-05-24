namespace VerticalSliceApi.Features.Games.Common
{
    public record GameResponse
    {
        public required string Name { get; set; }
        public char[][]? Board { get; set; }

        public GameState State { get; set; }
    }

    public static partial class GameResponseMapper
    {
        public static GameResponse ToResponse(this Game source)
        {
            return new GameResponse
            {
                Name = source.Name,
                State = source.State,
                Board = MapBoard(source.Board)
            };
        }

        public static IQueryable<GameResponse> ProjectToResponse(this IQueryable<Game> q)
        {
            return q.Select(g => new GameResponse
            {
                Name = g.Name,
                State = g.State,
                Board = null // Cannot map complex types in projections
            });
        }

        private static char[][]? MapBoard(Board? board) =>
            board?.Value.Select(row => row.Select(GetTileChar).ToArray()).ToArray();

        private static char GetTileChar(Tile tile) =>
            tile switch
            {
                Tile.Empty => ' ',
                Tile.X => 'X',
                Tile.O => 'O',
                _ => '?',
            };
    }
}
