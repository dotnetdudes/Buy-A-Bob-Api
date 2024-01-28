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
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags");
                return TypedResults.Json(productTags);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags where Deleted IS NULL");
                return TypedResults.Json(productTags);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<ProductTag>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productTag = await db.QueryFirstOrDefaultAsync<ProductTag>("SELECT * FROM ProductTags WHERE Id = @id", new { id });
                
                return productTag is null ? TypedResults.NotFound() : TypedResults.Json(productTag);
            });

            // get by product id
            group.MapGet("/product/{id}", async Task<Results<JsonHttpResult<IEnumerable<ProductTag>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags WHERE ProductId = @id", new { id });
                
                return productTags is null ? TypedResults.NotFound() : TypedResults.Json(productTags);
            });

            // get active by product id
            group.MapGet("/product/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<ProductTag>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags WHERE ProductId = @id AND Deleted IS NULL", new { id });
                
                return productTags is null ? TypedResults.NotFound() : TypedResults.Json(productTags);
            });

            // get by tag id
            group.MapGet("/tag/{id}", async Task<Results<JsonHttpResult<IEnumerable<ProductTag>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags WHERE TagId = @id", new { id });
                
                return productTags is null ? TypedResults.NotFound() : TypedResults.Json(productTags);
            });

            // get active by tag id
            group.MapGet("/tag/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<ProductTag>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productTags = await db.QueryAsync<ProductTag>("SELECT * FROM ProductTags WHERE TagId = @id AND Deleted IS NULL", new { id });
                // return TypedResults.Json(productTags);
                return productTags is null ? TypedResults.NotFound() : TypedResults.Json(productTags);
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
                    WHERE id = @Id", productTag);
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE ProductTags SET Deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
