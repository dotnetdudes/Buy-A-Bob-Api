using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class CartEndpoints
    {
        public static RouteGroupBuilder MapCartEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var carts = await db.QueryAsync<Cart>("SELECT * FROM Carts where Deleted IS NOT NULL");
                return TypedResults.Json(carts);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var cart = await db.QueryFirstOrDefaultAsync<Cart>("SELECT * FROM Carts WHERE Id = @id", new { id });
                return TypedResults.Json(cart);
            });

            group.MapPost("/", async Task<Results<Created<Cart>, NotFound, ValidationProblem>> (IValidator<Cart> validator, IDbConnection db, Cart cart) =>
            {
                // validate cart
                var validationResult = await validator.ValidateAsync(cart);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert cart into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO Carts (UserId, ProductId, Quantity)
                    VALUES (@UserId, @ProductId, @Quantity);
                    SELECT last_insert_rowid();", cart);
                return TypedResults.Created($"/carts/{cart.Id}", cart);
            });

            group.MapPut("/{id}", async Task<Results<Ok<Cart>, NotFound, ValidationProblem, BadRequest>> (IValidator<Cart> validator, IDbConnection db, string id, Cart cart) =>
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
                // validate cart
                var validationResult = await validator.ValidateAsync(cart);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update cart in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE Carts
                    SET UserId = @UserId, ProductId = @ProductId, Quantity = @Quantity
                    WHERE Id = @Id", cart);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(cart);
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
                // delete cart from database
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM Carts
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
