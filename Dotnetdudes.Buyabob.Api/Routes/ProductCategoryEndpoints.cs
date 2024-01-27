using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class ProductCategoryEndpoints
    {
        public static RouteGroupBuilder MapProductCategoryEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var productCategories = await db.QueryAsync<ProductCategory>("SELECT * FROM ProductCategories where Deleted IS NOT NULL");
                return TypedResults.Json(productCategories);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var productCategory = await db.QueryFirstOrDefaultAsync<ProductCategory>("SELECT * FROM ProductCategories WHERE Id = @id", new { id });
                return TypedResults.Json(productCategory);
            });

            group.MapPost("/", async Task<Results<Created<ProductCategory>, NotFound, ValidationProblem>> (IValidator<ProductCategory> validator, IDbConnection db, ProductCategory productCategory) =>
            {
                // validate productCategory
                var validationResult = await validator.ValidateAsync(productCategory);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert productCategory into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO ProductCategories (ProductId, CategoryId)
                    VALUES (@ProductId, @CategoryId);
                    SELECT last_insert_rowid();", productCategory);
                return TypedResults.Created($"/productCategories/{productCategory.Id}", productCategory);
            });

            group.MapPut("/{id}", async Task<Results<Ok<ProductCategory>, NotFound, ValidationProblem, BadRequest>> (IValidator<ProductCategory> validator, IDbConnection db, string id, ProductCategory productCategory) =>
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
                // validate productCategory
                var validationResult = await validator.ValidateAsync(productCategory);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update productCategory in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE ProductCategories
                    SET ProductId = @ProductId, CategoryId = @CategoryId
                    WHERE Id = @Id", productCategory);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(productCategory);
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
                // delete productCategory from database
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM ProductCategories
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
