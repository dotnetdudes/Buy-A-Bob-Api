using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class CartItemEndpoints
    {
        public static RouteGroupBuilder MapCartItemEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var cartItems = await db.QueryAsync<CartItem>("SELECT * FROM CartItems where Deleted IS NOT NULL");
                return TypedResults.Json(cartItems);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var cartItem = await db.QueryFirstOrDefaultAsync<CartItem>("SELECT * FROM CartItems WHERE Id = @id", new { id });
                return TypedResults.Json(cartItem);
            });

            group.MapPost("/", async Task<Results<Created<CartItem>, NotFound, ValidationProblem>> (IValidator<CartItem> validator, IDbConnection db, CartItem cartItem) =>
            {
                // validate cartItem
                var validationResult = await validator.ValidateAsync(cartItem);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert cartItem into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO CartItems (CartId, ProductId, Quantity)
                    VALUES (@CartId, @ProductId, @Quantity);
                    SELECT last_insert_rowid();", cartItem);
                return TypedResults.Created($"/cartItems/{cartItem.Id}", cartItem);
            });

            group.MapPut("/{id}", async Task<Results<Ok<CartItem>, NotFound, ValidationProblem, BadRequest>> (IValidator<CartItem> validator, IDbConnection db, string id, CartItem cartItem) =>
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
                // validate cartItem
                var validationResult = await validator.ValidateAsync(cartItem);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update cartItem in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE CartItems
                    SET CartId = @CartId,
                        ProductId = @ProductId,
                        Quantity = @Quantity
                    WHERE Id = @Id", cartItem);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(cartItem);
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
                // delete cartItem from database
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM CartItems
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
