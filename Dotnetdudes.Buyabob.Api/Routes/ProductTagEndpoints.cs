using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class ProductTagEndpoints
    {
        public static RouteGroupBuilder MapProductTagEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags where Deleted IS NOT NULL");
                return TypedResults.Json(productTags);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var productTag = await db.QueryFirstOrDefaultAsync<ProductTag>("SELECT * FROM ProductTags WHERE Id = @id", new { id });
                return TypedResults.Json(productTag);
            });

            group.MapPost("/", async Task<Results<Created<ProductTag>, NotFound, ValidationProblem>> (IValidator<ProductTag> validator, IDbConnection db, ProductTag productTag) =>
            {
                // validate productTag
                var validationResult = await validator.ValidateAsync(productTag);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert productTag into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO ProductTags (ProductId, TagId)
                    VALUES (@ProductId, @TagId);
                    SELECT last_insert_rowid();", productTag);
                return TypedResults.Created($"/productTags/{productTag.Id}", productTag);
            });

            group.MapPut("/{id}", async Task<Results<Ok<ProductTag>, NotFound, ValidationProblem, BadRequest>> (IValidator<ProductTag> validator, IDbConnection db, string id, ProductTag productTag) =>
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
                // validate productTag
                var validationResult = await validator.ValidateAsync(productTag);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update productTag in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE ProductTags
                    SET ProductId = @ProductId, TagId = @TagId
                    WHERE Id = @Id", productTag);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(productTag);
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
                // delete productTag from database
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM ProductTags
                    WHERE Id = @Id", new { Id = number });
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
