﻿namespace VerticalSliceApi.Features.Games
{
    public sealed record NewGameRequest(string Name);

    internal sealed class NewGameRequestValidator : AbstractValidator<NewGameRequest>
    {
        public NewGameRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(Game.MaxNameLength);
        }
    }

    internal sealed class NewGameCommand(AppDbContext db)
        : Endpoint<NewGameRequest, Results<Created, BadRequest>>
    {
        public override void Configure()
        {
            Post("/games");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            NewGameRequest request,
            CancellationToken cancellationToken
        )
        {
            var game = new Game((GameId)Guid.CreateVersion7(), request.Name);
            db.Add(game);
            await db.SaveChangesAsync(cancellationToken);

            await SendResultAsync(TypedResults.Created($"/games/{game.Id}"));
        }
    }
}
