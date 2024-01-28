using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class TagEndpoints
    {
        public static RouteGroupBuilder MapTagEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var tags = await db.QueryAsync<Tag>("SELECT * FROM Tags");
                return TypedResults.Json(tags);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var tags = await db.QueryAsync<Tag>("SELECT * FROM Tags where Deleted IS NULL");
                return TypedResults.Json(tags);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Tag>, NotFound, BadRequest>>(IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var tag = await db.QueryFirstOrDefaultAsync<Tag>("SELECT * FROM Tags WHERE Id = @id", new { id });
                
                return tag is null ? TypedResults.NotFound() : TypedResults.Json(tag);
            });

            group.MapPost("/", async Task<Results<Created<Tag>, NotFound, ValidationProblem>> (IValidator<Tag> validator, IDbConnection db, Tag tag) =>
            {
                // validate tag
                var validationResult = await validator.ValidateAsync(tag);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert tag into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO Tags (Name)
                    VALUES (@Name);
                    SELECT last_insert_rowid();", tag);
                return TypedResults.Created($"/tags/{tag.Id}", tag);
            });

            group.MapPut("/{id}", async Task<Results<Ok<Tag>, NotFound, ValidationProblem, BadRequest>> (IValidator<Tag> validator, IDbConnection db, string id, Tag tag) =>
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
                // validate tag
                var validationResult = await validator.ValidateAsync(tag);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update tag in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE Tags
                    SET Name = @Name
                    WHERE id = @Id", tag);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(tag);
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
                // delete tag from database
                var rowsAffected = await db.ExecuteAsync(@"UPDATE Tags SET Deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
