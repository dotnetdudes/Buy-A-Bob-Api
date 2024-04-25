using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class ShippingTypeEndpoints
    {
        public static RouteGroupBuilder MapShippingTypeEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var shippingtypes = await db.QueryAsync<ShippingType>("SELECT * FROM shippingtypes");
                return TypedResults.Json(shippingtypes);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var shippingtypes = await db.QueryAsync<ShippingType>("SELECT * FROM shippingtypes where deleted IS NULL");
                return TypedResults.Json(shippingtypes);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<ShippingType>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var shippingType = await db.QueryFirstOrDefaultAsync<ShippingType>("SELECT * FROM shippingtypes WHERE id = @id", new { id });

                return shippingType is null ? TypedResults.NotFound() : TypedResults.Json(shippingType);
            });

            group.MapPost("/", async Task<Results<Created<ShippingType>, NotFound, ValidationProblem>> (IValidator<ShippingType> validator, IDbConnection db, ShippingType shippingType) =>
            {
                // validate shippingType
                var validationResult = await validator.ValidateAsync(shippingType);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert shippingType into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO shippingtypes (name)
                    VALUES (@Name) returning id;", shippingType);
                return TypedResults.Created($"/shippingtypes/{shippingType.Id}", shippingType);
            }).RequireAuthorization("BobAdmin");

            group.MapPut("/{id}", async Task<Results<Ok<ShippingType>, NotFound, ValidationProblem, BadRequest>> (IValidator<ShippingType> validator, IDbConnection db, string id, ShippingType shippingType) =>
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
                // validate shippingType
                var validationResult = await validator.ValidateAsync(shippingType);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                shippingType.Updated = DateTime.UtcNow;
                // update shippingType in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE shippingtypes
                    SET name = @Name
                    WHERE id = @Id", shippingType);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(shippingType);
            }).RequireAuthorization("BobAdmin");

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
                // delete shippingType from database
                var rowsAffected = await db.ExecuteAsync(@"UPDATE shippingtypes SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            }).RequireAuthorization("BobAdmin");

            return group;

        }
    }
}
