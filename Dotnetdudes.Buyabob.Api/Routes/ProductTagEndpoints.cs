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
                var producttags = await db.QueryAsync<ProductTag>("SELECT * FROM producttags");
                return TypedResults.Json(producttags);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var producttags = await db.QueryAsync<ProductTag>("SELECT * FROM producttags where deleted IS NULL");
                return TypedResults.Json(producttags);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<ProductTag>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productTag = await db.QueryFirstOrDefaultAsync<ProductTag>("SELECT * FROM producttags WHERE id = @id", new { id });

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
                var producttags = await db.QueryAsync<ProductTag>("SELECT * FROM producttags WHERE productid = @id", new { id });

                return producttags is null ? TypedResults.NotFound() : TypedResults.Json(producttags);
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
                var producttags = await db.QueryAsync<ProductTag>("SELECT * FROM producttags WHERE productid = @id AND deleted IS NULL", new { id });

                return producttags is null ? TypedResults.NotFound() : TypedResults.Json(producttags);
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
                var producttags = await db.QueryAsync<ProductTag>("SELECT * FROM producttags WHERE tagid = @id", new { id });

                return producttags is null ? TypedResults.NotFound() : TypedResults.Json(producttags);
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
                var producttags = await db.QueryAsync<ProductTag>("SELECT * FROM producttags WHERE tagid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(producttags);
                return producttags is null ? TypedResults.NotFound() : TypedResults.Json(producttags);
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
                    INSERT INTO producttags (productid, tagid)
                    VALUES (@ProductId, @TagId) returning id;", productTag);
                return TypedResults.Created($"/producttags/{productTag.Id}", productTag);
            }).RequireAuthorization("BobAdmin");

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
                    UPDATE producttags
                    SET productid = @ProductId, tagid = @TagId
                    WHERE id = @Id", productTag);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(productTag);
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
                // delete productTag from database
                var rowsAffected = await db.ExecuteAsync(@"UPDATE producttags SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
