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
                var shippingaddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM shippingaddresses");
                return TypedResults.Json(shippingaddresses);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var shippingaddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM shippingaddresses where deleted IS NULL");
                return TypedResults.Json(shippingaddresses);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<ShippingAddress>, BadRequest, NotFound>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var shippingAddress = await db.QueryFirstOrDefaultAsync<ShippingAddress>("SELECT * FROM shippingaddresses WHERE id = @id", new { id });
                // return TypedResults.Json(shippingAddress);
                return shippingAddress is null ? TypedResults.NotFound() : TypedResults.Json(shippingAddress);
            });

            // get by customer id
            group.MapGet("/customer/{id}", async Task<Results<JsonHttpResult<IEnumerable<ShippingAddress>>, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var shippingaddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM shippingaddresses WHERE customerid = @id", new { id });
                return TypedResults.Json(shippingaddresses);
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
                var shippingaddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM shippingaddresses WHERE customerid = @id AND deleted IS NULL", new { id });
                return TypedResults.Json(shippingaddresses);
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
                    INSERT INTO shippingaddresses (name)
                    VALUES (@Name) returning id;", shippingAddress);
                return TypedResults.Created($"/shippingaddresses/{shippingAddress.Id}", shippingAddress);
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE shippingaddresses SET deleted = @date WHERE id = @id", new { date = DateTime.Now, id });
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
