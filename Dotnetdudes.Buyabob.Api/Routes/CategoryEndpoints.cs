using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class CategoryEndpoints
    {
        public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var categories = await db.QueryAsync<Category>("SELECT * FROM categories");
                return TypedResults.Json(categories);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var categories = await db.QueryAsync<Category>("SELECT * FROM categories where deleted IS NULL");
                return TypedResults.Json(categories);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Category>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var category = await db.QueryFirstOrDefaultAsync<Category>("SELECT * FROM categories WHERE id = @id", new { id });
                // return TypedResults.Json(category);
                return category is null ? TypedResults.NotFound() : TypedResults.Json(category);
            });

            group.MapPost("/", async Task<Results<Created<Category>, NotFound, ValidationProblem>> (IValidator<Category> validator, IDbConnection db, Category category) =>
            {
                // validate category
                var validationResult = await validator.ValidateAsync(category);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert category into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO categories (name)
                    VALUES (@Name) returning id;", category);
                return TypedResults.Created($"/categories/{category.Id}", category);
            }).RequireAuthorization("BobAdmin");

            group.MapPut("/{id}", async Task<Results<Ok<Category>, NotFound, ValidationProblem, BadRequest>> (IValidator<Category> validator, IDbConnection db, string id, Category category) =>
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
                // validate category
                var validationResult = await validator.ValidateAsync(category);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update category in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE categories
                    SET name = @Name
                    WHERE id = @Id", category);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(category);
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
                // delete category from database
                var rowsAffected = await db.ExecuteAsync(@"
                    Update categories
                    set deleted = @date
                    WHERE id = @id", new { date = DateTime.UtcNow, id });
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
