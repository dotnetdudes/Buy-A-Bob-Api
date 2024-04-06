using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;



namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class CustomerEndpoints
    {
        public static RouteGroupBuilder MapCustomerEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var customers = await db.QueryAsync<Customer>("SELECT * FROM customers");
                return TypedResults.Json(customers);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var customers = await db.QueryAsync<Customer>("SELECT * FROM customers where deleted IS NULL");
                return TypedResults.Json(customers);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Customer>, NotFound, BadRequest>>  (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var customer = await db.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM customers WHERE id = @id", new { id });
                // return TypedResults.Json(customer);
                return customer is null ? TypedResults.NotFound() : TypedResults.Json(customer);
            });

            group.MapPost("/", async Task<Results<Created<Customer>, NotFound, ValidationProblem>> (IValidator<Customer> validator, IDbConnection db, Customer customer) =>
            {
                // validate customer
                var validationResult = await validator.ValidateAsync(customer);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert customer into database
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO customers (name)
                    VALUES (@Name) returning id;", customer);
                return TypedResults.Created($"/customers/{customer.Id}", customer);
            });

            group.MapPut("/{id}", async Task<Results<Ok<Customer>, NotFound, ValidationProblem, BadRequest>> (IValidator<Customer> validator, IDbConnection db, string id, Customer customer) =>
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
                // validate customer
                var validationResult = await validator.ValidateAsync(customer);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update customer in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE customers SET firstname = @FirstName, lastname = @LastName, email = @Email WHERE id = @Id", customer);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(customer);
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
                // delete customer from database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE customers
                    SET deleted = @date
                    WHERE id = @id", new { date = DateTime.UtcNow, id });
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
