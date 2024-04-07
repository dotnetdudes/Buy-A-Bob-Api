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
                var productcategories = await db.QueryAsync<ProductCategory>("SELECT * FROM productcategories");
                return TypedResults.Json(productcategories);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var productcategories = await db.QueryAsync<ProductCategory>("SELECT * FROM productcategories where deleted IS NULL");
                return TypedResults.Json(productcategories);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<ProductCategory>, NotFound, BadRequest>>(IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productCategory = await db.QueryFirstOrDefaultAsync<ProductCategory>("SELECT * FROM productcategories WHERE id = @id", new { id });
                
                return productCategory is null ? TypedResults.NotFound() : TypedResults.Json(productCategory);
            });

            // get by product id
            group.MapGet("/product/{id}", async Task<Results<JsonHttpResult<IEnumerable<ProductCategory>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productcategories = await db.QueryAsync<ProductCategory>("SELECT * FROM productcategories WHERE productid = @id", new { id });
                
                return productcategories is null ? TypedResults.NotFound() : TypedResults.Json(productcategories);
            });

            // get active by product id
            group.MapGet("/product/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<ProductCategory>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productcategories = await db.QueryAsync<ProductCategory>("SELECT * FROM productcategories WHERE productid = @id AND deleted IS NULL", new { id });
                
                return productcategories is null ? TypedResults.NotFound() : TypedResults.Json(productcategories);
            });

            // get by category id
            group.MapGet("/category/{id}", async Task<Results<JsonHttpResult<IEnumerable<ProductCategory>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productcategories = await db.QueryAsync<ProductCategory>("SELECT * FROM productcategories WHERE categoryid = @id", new { id });
                
                return productcategories is null ? TypedResults.NotFound() : TypedResults.Json(productcategories);
            });

            // get active by category id
            group.MapGet("/category/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<ProductCategory>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var productcategories = await db.QueryAsync<ProductCategory>("SELECT * FROM productcategories WHERE categoryid = @id AND deleted IS NULL", new { id });
                
                return productcategories is null ? TypedResults.NotFound() : TypedResults.Json(productcategories);
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
                    INSERT INTO productcategories (productid, categoryid)
                    VALUES (@ProductId, @CategoryId) returning id;", productCategory);
                return TypedResults.Created($"/productcategories/{productCategory.Id}", productCategory);
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
                    UPDATE productcategories
                    SET productid = @ProductId, categoryid = @CategoryId
                    WHERE id = @Id", productCategory);
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE productcategories SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
