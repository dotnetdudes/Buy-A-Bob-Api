using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static  class ProductEndPoints
    {
        public static RouteGroupBuilder MapProductEndPoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var products = await db.QueryAsync<Product>("SELECT * FROM Products where Deleted IS NOT NULL");
                return TypedResults.Json(products);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var product = await db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @id", new { id });
                return TypedResults.Json(product);
            });

            group.MapPost("/", async Task<Results<Created<Product>, NotFound, ValidationProblem>> (IValidator<Product> validator, IDbConnection db, Product product) =>
            {
                // validate product
                var validationResult = await validator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert product into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO Products (Name, Description, Price, ImageUrl)
                    VALUES (@Name, @Description, @Price, @ImageUrl);
                    SELECT last_insert_rowid();", product);
                return TypedResults.Created($"/products/{product.Id}", product);
            });

            group.MapPut("/{id}", async Task<Results<Ok<Product>, NotFound, ValidationProblem, BadRequest>> (IValidator<Product> validator, IDbConnection db, string id, Product product) =>
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
                // validate product
                var validationResult = await validator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update product in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE Products
                    SET Name = @Name, Description = @Description, Price = @Price, ImageUrl = @ImageUrl
                    WHERE Id = @Id", product);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(product);
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
                // delete product from database
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM Products
                    WHERE Id = @number", new { number });
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
