﻿using Dapper;
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
                var cartitems = await db.QueryAsync<CartItem>("SELECT * FROM cartitems");
                return TypedResults.Json(cartitems);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var cartitems = await db.QueryAsync<CartItem>("SELECT * FROM cartitems where deleted IS NULL");
                return TypedResults.Json(cartitems);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<CartItem>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var cartItem = await db.QueryFirstOrDefaultAsync<CartItem>("SELECT * FROM cartitems WHERE id = @id", new { id });
                return cartItem is null ? TypedResults.NotFound() : TypedResults.Json(cartItem);
            });

            // get by cart id
            group.MapGet("/cart/{id}", async Task<Results<JsonHttpResult<IEnumerable<CartItem>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var cartitems = await db.QueryAsync<CartItem>("SELECT * FROM cartitems WHERE cartid = @id", new { id });
                // return TypedResults.Json(cartitems);
                return cartitems is null ? TypedResults.NotFound() : TypedResults.Json(cartitems);
            });

            // get active by cart id
            group.MapGet("/cart/{id}/active", async Task<Results<JsonHttpResult<IEnumerable<CartItem>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var cartitems = await db.QueryAsync<CartItem>("SELECT * FROM cartitems where cartid = @id AND deleted IS NULL", new { id });
                // return TypedResults.Json(cartitems);
                return cartitems is null ? TypedResults.NotFound() : TypedResults.Json(cartitems);
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
                    insert into cartitems (cartid, productid, quantity)
                    VALUES (@CartId, @ProductId, @Quantity) returning id;", cartItem);
                return TypedResults.Created($"/cartitems/{cartItem.Id}", cartItem);
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
                cartItem.Updated = DateTime.UtcNow;
                // update cartItem in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE cartitems
                    SET cartid = @CartId,
                        productid = @ProductId,
                        quantity = @Quantity
                    WHERE id = @Id", cartItem);
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
                var rowsAffected = await db.ExecuteAsync(@"UPDATE cartitems SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
