using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class ShippingAddressEndpoints
    {
        public static RouteGroupBuilder MapShippingAddressEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var shippingAddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM ShippingAddresses");
                return TypedResults.Json(shippingAddresses);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var shippingAddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM ShippingAddresses where Deleted IS NULL");
                return TypedResults.Json(shippingAddresses);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<ShippingAddress>, BadRequest, NotFound>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var shippingAddress = await db.QueryFirstOrDefaultAsync<ShippingAddress>("SELECT * FROM ShippingAddresses WHERE Id = @id", new { id });
                // return TypedResults.Json(shippingAddress);
                return shippingAddress is null ? TypedResults.NotFound() : TypedResults.Json(shippingAddress);
            });

            // get by customer id
            group.MapGet("/customer/{id}", async Task<Results<JsonHttpResult<IEnumerable<ShippingAddress>>, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var shippingAddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM ShippingAddresses WHERE CustomerId = @id", new { id });
                return TypedResults.Json(shippingAddresses);
            });

            // get active by customer id
            group.MapGet("/customer/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<ShippingAddress>>, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var shippingAddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM ShippingAddresses WHERE CustomerId = @id AND Deleted IS NULL", new { id});
                return TypedResults.Json(shippingAddresses);
            });

            group.MapPost("/", async Task<Results<Created<ShippingAddress>, NotFound, ValidationProblem>> (IValidator<ShippingAddress> validator, IDbConnection db, ShippingAddress shippingAddress) =>
            {
                // validate shippingAddress
                var validationResult = await validator.ValidateAsync(shippingAddress);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert shippingAddress into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO ShippingAddresses (Name)
                    VALUES (@Name);
                    SELECT last_insert_rowid();", shippingAddress);
                return TypedResults.Created($"/shippingAddresses/{shippingAddress.Id}", shippingAddress);
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
                // delete shippingAddress from database
                var rowsAffected = await db.ExecuteAsync(@"UPDATE ShippingAddresses SET Deleted = @date WHERE id = @id", new { date = DateTime.Now, id });
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
