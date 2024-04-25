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
                var carts = await db.QueryAsync<Cart>("SELECT * FROM carts");
                return TypedResults.Json(carts);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var carts = await db.QueryAsync<Cart>("SELECT * FROM carts where deleted IS NULL");
                return TypedResults.Json(carts);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Cart>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var cart = await db.QueryFirstOrDefaultAsync<Cart>("SELECT * FROM carts WHERE id = @id", new { id });
                // return TypedResults.Json(cart);
                return cart is null ? TypedResults.NotFound() : TypedResults.Json(cart);
            });

            // get by customer id
            group.MapGet("/customer/{id}", async Task<Results<JsonHttpResult<IEnumerable<Cart>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var carts = await db.QueryAsync<Cart>("SELECT * FROM carts WHERE customerid = @id", new { id });
                // return TypedResults.Json(carts);
                return carts is null ? TypedResults.NotFound() : TypedResults.Json(carts);
            });

            // get active by customer id
            group.MapGet("/customer/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Cart>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var carts = await db.QueryAsync<Cart>("SELECT * FROM carts where customerid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(carts);
                return carts is null ? TypedResults.NotFound() : TypedResults.Json(carts);
            });

            // get by status id
            group.MapGet("/status/{id}", async Task<Results<JsonHttpResult<IEnumerable<Cart>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var carts = await db.QueryAsync<Cart>("SELECT * FROM carts where statusid = @id", new { id });
                // return TypedResults.Json(carts);
                return carts is null ? TypedResults.NotFound() : TypedResults.Json(carts);
            });

            // get active by status id
            group.MapGet("/status/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Cart>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var carts = await db.QueryAsync<Cart>("SELECT * FROM carts where statusid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(carts);
                return carts is null ? TypedResults.NotFound() : TypedResults.Json(carts);
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
                    INSERT INTO carts (userid, productid, quantity)
                    VALUES (@UserId, @ProductId, @Quantity) returning id;", cart);
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
                cart.Updated = DateTime.UtcNow;
                // update cart in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE carts
                    SET userid = @UserId, productid = @ProductId, quantity = @Quantity
                    WHERE id = @Id", cart);
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE carts set deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
