﻿using VerticalSliceApi.Features.Games.Common;

namespace VerticalSliceApi.Features.Games
{
    public sealed record PlayTurnRequest(GameId GameId, BoardPosition BoardPosition, Tile Player);

    internal sealed class PlayTurnCommand(AppDbContext db)
        : Endpoint<PlayTurnRequest, Results<Ok<GameResponse>, NotFound>>
    {
        public override void Configure()
        {
            Post("/games/{GameId}/play-turn");
            Summary(x =>
            {
                x.Description = "Make a move in the game";
            });
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            PlayTurnRequest request,
            CancellationToken cancellationToken
        )
        {
            var game = await db.FindAsync<Game>(request.GameId, cancellationToken);

            if (game is null)
            {
                await SendResultAsync(TypedResults.NotFound());
                return;
            }

            game.MakeMove(request.BoardPosition, request.Player);
            await db.SaveChangesAsync(cancellationToken);

            var output = game.ToResponse();
            await SendResultAsync(TypedResults.Ok(output));
        }
    }
}
