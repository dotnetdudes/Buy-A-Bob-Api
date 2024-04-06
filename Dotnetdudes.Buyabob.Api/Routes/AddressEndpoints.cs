using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class AddressEndpoints
    {
        public static RouteGroupBuilder MapAddressEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var addresses = await db.QueryAsync<Address>("SELECT * FROM addresses");
                return TypedResults.Json(addresses);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var addresses = await db.QueryAsync<Address>("SELECT * FROM addresses where deleted IS NULL");
                return TypedResults.Json(addresses);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Address>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var address = await db.QueryFirstOrDefaultAsync<Address>("SELECT * FROM addresses WHERE id = @id", new { id });
                // return TypedResults.Json(address);
                return address is null ? TypedResults.NotFound() : TypedResults.Json(address);
            });

            // get by customer id
            group.MapGet("/customer/{id}", async Task<Results<JsonHttpResult<IEnumerable<Address>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var addresses = await db.QueryAsync<Address>("SELECT * FROM addresses WHERE customerid = @id", new { id });
                // return TypedResults.Json(addresses);
                return addresses is null ? TypedResults.NotFound() : TypedResults.Json(addresses);
            });
            
            // get active by customer id
            group.MapGet("/customer/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<Address>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var addresses = await db.QueryAsync<Address>("SELECT * FROM addresses WHERE customerid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(addresses);
                return addresses is null ? TypedResults.NotFound() : TypedResults.Json(addresses);
            });

            group.MapPost("/", async Task<Results<Created<Address>, NotFound, ValidationProblem>> (IValidator<Address> validator, IDbConnection db, Address address) =>
            {
                // validate address
                var validationResult = await validator.ValidateAsync(address);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert address into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO addresses (street, city, state, postcode, country)
                    VALUES (@Street, @City, @State, @Postcode, @Country) returning id;", address);
                return TypedResults.Created($"/addresses/{address.Id}", address);
            });

            group.MapPut("/{id}", async Task<Results<Ok<Address>, NotFound, ValidationProblem, BadRequest>> (IValidator<Address> validator, IDbConnection db, string id, Address address) =>
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
                // validate address
                var validationResult = await validator.ValidateAsync(address);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                address.Updated = DateTime.UtcNow;
                // update address in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE addresses
                    SET street = @Street, city = @City, state = @State, postcode = @Postcode, country = @Country, updated = @Updated
                    WHERE id = @Id", address);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(address);
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
                // delete address from database
                var rowsAffected = await db.ExecuteAsync("UPDATE addresses SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id});
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
