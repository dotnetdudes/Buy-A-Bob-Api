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
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders");
                return TypedResults.Json(orders);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders where deleted IS NULL");
                return TypedResults.Json(orders);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Order>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var order = await db.QueryFirstOrDefaultAsync<Order>("SELECT * FROM orders WHERE id = @id", new { id });
                // return TypedResults.Json(order);
                return order is null ? TypedResults.NotFound() : TypedResults.Json(order);
            });

            // get by customer id
            group.MapGet("/customer/{id}", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE customerid = @id", new { id });
                
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get active by customer id
            group.MapGet("/customer/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE customerid = @id AND deleted IS NULL", new { id });
                
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get by status id
            group.MapGet("/status/{id}", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE statusid = @id", new { id });
                // return TypedResults.Json(orders);
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get active by status id
            group.MapGet("/status/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE statusid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(orders);
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get by shipping type id
            group.MapGet("/shippingType/{id}", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE shippingtypeid = @id", new { id });
                // return TypedResults.Json(orders);
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get active by shipping type id
            group.MapGet("/shippingType/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE shippingtypeid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(orders);
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get by shipping address id
            group.MapGet("/shippingAddress/{id}", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE shippingaddressid = @id", new { id });
                // return TypedResults.Json(orders);
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
            });

            // get active by shipping address id
            group.MapGet("/shippingAddress/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Order>>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success) 
                {
                    return TypedResults.BadRequest();
                }
                var orders = await db.QueryAsync<Order>("SELECT * FROM orders WHERE shippingaddressid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(orders);
                return orders is null ? TypedResults.NotFound() : TypedResults.Json(orders);
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
                    INSERT INTO orders (userid, productid, quantity)
                    VALUES (@UserId, @ProductId, @Quantity) returning id;", order);
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
                    UPDATE orders
                    SET statusid = @StatusId,
                        subtotal = @SubTotal,
                        tax = @Tax,
                        shippingtypeid = @ShippingTypeId,
                        shipping = @Shipping,
                        shippingaddressid = @ShippingAddressId,
                        contactname = @ContactName,
                        contactphone = @ContactPhone,
                        total = @Total,
                        datepurchased = @DatePurchased,
                        dateshipped = @DateShipped,
                        deleted = @Deleted
                    WHERE id = @Id", order);
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE orders SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
