using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class OrderEndpoints
    {
        public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var orders = await db.QueryAsync<Order>("SELECT * FROM Orders where Deleted IS NOT NULL");
                return TypedResults.Json(orders);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var order = await db.QueryFirstOrDefaultAsync<Order>("SELECT * FROM Orders WHERE Id = @id", new { id });
                return TypedResults.Json(order);
            });

            group.MapPost("/", async Task<Results<Created<Order>, NotFound, ValidationProblem>> (IValidator<Order> validator, IDbConnection db, Order order) =>
            {
                // validate order
                var validationResult = await validator.ValidateAsync(order);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert order into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO Orders (UserId, ProductId, Quantity)
                    VALUES (@UserId, @ProductId, @Quantity);
                    SELECT last_insert_rowid();", order);
                return TypedResults.Created($"/orders/{order.Id}", order);
            });

            group.MapPut("/{id}", async Task<Results<Ok<Order>, NotFound, ValidationProblem, BadRequest>> (IValidator<Order> validator, IDbConnection db, string id, Order order) =>
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
                // validate order
                var validationResult = await validator.ValidateAsync(order);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update order in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE Orders
                    SET UserId = @UserId,
                        ProductId = @ProductId,
                        Quantity = @Quantity
                    WHERE Id = @Id", order);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(order);
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
                // delete order from database
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM Orders
                    WHERE Id = @Id", new { Id = id });
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
