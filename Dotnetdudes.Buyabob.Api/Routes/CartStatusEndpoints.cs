using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class CartStatusEndpoints
    {
        public static RouteGroupBuilder MapCartStatusEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var cartStatuses = await db.QueryAsync<CartStatus>("SELECT * FROM CartStatuses where Deleted IS NOT NULL");
                return TypedResults.Json(cartStatuses);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var cartStatus = await db.QueryFirstOrDefaultAsync<CartStatus>("SELECT * FROM CartStatuses WHERE Id = @id", new { id });
                return TypedResults.Json(cartStatus);
            });

            group.MapPost("/", async Task<Results<Created<CartStatus>, NotFound, ValidationProblem>> (IValidator<CartStatus> validator, IDbConnection db, CartStatus cartStatus) =>
            {
                // validate cartStatus
                var validationResult = await validator.ValidateAsync(cartStatus);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert cartStatus into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO CartStatuses (Name)
                    VALUES (@Name);
                    SELECT last_insert_rowid();", cartStatus);
                return TypedResults.Created($"/cartStatuses/{cartStatus.Id}", cartStatus);
            });

            group.MapPut("/{id}", async Task<Results<Ok<CartStatus>, NotFound, ValidationProblem, BadRequest>> (IValidator<CartStatus> validator, IDbConnection db, string id, CartStatus cartStatus) =>
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
                    UPDATE CartStatuses
                    SET Name = @Name
                    WHERE Id = @Id", cartStatus);
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
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM CartStatuses
                    WHERE Id = @Id", new { id });
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
