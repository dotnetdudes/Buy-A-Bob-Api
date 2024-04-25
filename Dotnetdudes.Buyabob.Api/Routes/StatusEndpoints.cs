using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class StatusEndpoints
    {
        public static RouteGroupBuilder MapStatusEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var cartstatuses = await db.QueryAsync<Status>("SELECT * FROM statuses");
                return TypedResults.Json(cartstatuses);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var cartstatuses = await db.QueryAsync<Status>("SELECT * FROM statuses where deleted IS NULL");
                return TypedResults.Json(cartstatuses);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Status>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var cartStatus = await db.QueryFirstOrDefaultAsync<Status>("SELECT * FROM statuses WHERE Id = @id", new { id });

                return cartStatus is null ? TypedResults.NotFound() : TypedResults.Json(cartStatus);
            });

            group.MapPost("/", async Task<Results<Created<Status>, NotFound, ValidationProblem>> (IValidator<Status> validator, IDbConnection db,
            Status cartStatus) =>
            {
                // validate cartStatus
                var validationResult = await validator.ValidateAsync(cartStatus);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert cartStatus into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO statuses (name)
                    VALUES (@Name) returning id;", cartStatus);
                return TypedResults.Created($"/cartstatuses/{cartStatus.Id}", cartStatus);
            }).RequireAuthorization("BobAdmin");

            group.MapPut("/{id}", async Task<Results<Ok<Status>, NotFound, ValidationProblem, BadRequest>> (IValidator<Status> validator, IDbConnection db, string id, Status cartStatus) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                if (number < 1)
                {
                    return TypedResults.BadRequest();
                }
                // validate cartStatus
                var validationResult = await validator.ValidateAsync(cartStatus);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update cartStatus in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE statuses
                    SET name = @Name
                    WHERE id = @Id", cartStatus);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(cartStatus);
            }).RequireAuthorization("BobAdmin");

            group.MapDelete("/{id}", async Task<Results<NoContent, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                if (number < 1)
                {
                    return TypedResults.BadRequest();
                }
                // delete cartStatus from database
                var rowsAffected = await db.ExecuteAsync(@"UPDATE statuses SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            }).RequireAuthorization("BobAdmin");

            return group;
        }
    }
}
