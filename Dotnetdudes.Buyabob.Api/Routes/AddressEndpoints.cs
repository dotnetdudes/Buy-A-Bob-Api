﻿using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class AddressEndpoints
    {
        public static RouteGroupBuilder MapAddressEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var addresses = await db.QueryAsync<Address>("SELECT * FROM Addresses where Deleted IS NOT NULL");
                return TypedResults.Json(addresses);
            });

            group.MapGet("/{id}", async (IDbConnection db, int id) =>
            {
                var address = await db.QueryFirstOrDefaultAsync<Address>("SELECT * FROM Addresses WHERE Id = @id", new { id });
                return TypedResults.Json(address);
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
                    INSERT INTO Addresses (Street, City, State, Postcode, Country)
                    VALUES (@Street, @City, @State, @Postcode, @Country);
                    SELECT last_insert_rowid();", address);
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
                // update address in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE Addresses
                    SET Street = @Street, City = @City, State = @State, Postcode = @Postcode, Country = @Country
                    WHERE Id = @number", address);
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
                var rowsAffected = await db.ExecuteAsync("DELETE FROM Addresses WHERE Id = @id", new { number });
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