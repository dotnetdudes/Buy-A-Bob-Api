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
                var cartStatuses = await db.QueryAsync<Status>("SELECT * FROM Statuses");
                return TypedResults.Json(cartStatuses);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var cartStatuses = await db.QueryAsync<Status>("SELECT * FROM Statuses where Deleted IS NULL");
                return TypedResults.Json(cartStatuses);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Status>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var cartStatus = await db.QueryFirstOrDefaultAsync<Status>("SELECT * FROM Statuses WHERE Id = @id", new { id });
                
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
                    INSERT INTO Statuses (Name)
                    VALUES (@Name);
                    SELECT last_insert_rowid();", cartStatus);
                return TypedResults.Created($"/cartStatuses/{cartStatus.Id}", cartStatus);
            });

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
                    UPDATE Statuses
                    SET Name = @Name
                    WHERE id = @Id", cartStatus);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(cartStatus);
            });
            
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE Statuses SET Deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            });

            return group;
        }
    }
}
