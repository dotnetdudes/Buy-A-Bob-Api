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
                var shippingAddresses = await db.QueryAsync<ShippingAddress>("SELECT * FROM ShippingAddresses where Deleted IS NOT NULL");
                return TypedResults.Json(shippingAddresses);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var shippingAddress = await db.QueryFirstOrDefaultAsync<ShippingAddress>("SELECT * FROM ShippingAddresses WHERE Id = @id", new { id });
                return TypedResults.Json(shippingAddress);
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

            group.MapPut("/{id}", async Task<Results<Ok<ShippingAddress>, NotFound, ValidationProblem, BadRequest>> (IValidator<ShippingAddress> validator, IDbConnection db, string id, ShippingAddress shippingAddress) =>
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
                // validate shippingAddress
                var validationResult = await validator.ValidateAsync(shippingAddress);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update shippingAddress in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE ShippingAddresses
                    SET Name = @Name
                    WHERE Id = @Id", shippingAddress);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(shippingAddress);
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
                var rowsAffected = await db.ExecuteAsync(@"
                    DELETE FROM ShippingAddresses
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
