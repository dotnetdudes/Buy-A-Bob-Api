using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class ProductEndpoints
    {
        public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (IDbConnection db) =>
            {
                var products = await db.QueryAsync<Product>("SELECT * FROM products");
                return TypedResults.Json(products);
            });

            group.MapGet("/active", async (IDbConnection db) =>
            {
                var products = await db.QueryAsync<Product>("SELECT * FROM products where deleted is null and issold = false");
                return TypedResults.Json(products);
            });

            group.MapGet("/{id}", async Task<Results<JsonHttpResult<Product>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                // validate id
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                var product = await db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM products where id = @id", new { id });
                // return TypedResults.Json(product);
                return product is null ? TypedResults.NotFound() : TypedResults.Json(product);
            });

            group.MapPost("/", async Task<Results<Ok<Product>, ValidationProblem>> (IValidator<Product> validator, IDbConnection db, IConfiguration config, IFormFile? file, [FromForm] string name, [FromForm] decimal price, [FromForm] string description, [FromForm] decimal weight, [FromForm] decimal width, [FromForm] decimal height, [FromForm] decimal depth, [FromForm] int quantity) =>
            {
                string dir = "uploads/";
                dir = config["FileSaveOptions:SaveDirectory"] ?? dir;
                // create directory if it doesn't exist
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // rename file in secure fashion
                if (file is null)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]> { { "file", new string[] { "File is required" } } });
                }
                string fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(dir, fileName);
                // save file to disk
                using var stream = File.OpenWrite(filePath);
                await file.CopyToAsync(stream);
                // validate product
                var product = new Product { Name = name, Description = description,ImageUrl= fileName, Price = price, Weight = weight, Width = width, Height = height, Depth = depth, Quantity = quantity, Created = DateTime.UtcNow };
                var validationResult = await validator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // write database record
                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO products (name, description, price, imageurl, weight, width, height, depth, quantity, created)
                    VALUES (@Name, @Description, @Price, @ImageUrl, @Weight, @Width, @Height, @Depth, @Quantity, @Created) returning id;", product);
                product.Id = id;
                return TypedResults.Ok(product);
            }).RequireAuthorization("BobAdmin");

            group.MapPut("/{id}", async Task<Results<Ok<Product>, NotFound, ValidationProblem, BadRequest>> (IValidator<Product> validator, IDbConnection db, string id, Product product) =>
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
                // validate product
                var validationResult = await validator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                product.Updated = DateTime.UtcNow;
                // update product in database
                var rowsAffected = await db.ExecuteAsync(@"
                    UPDATE products
                    SET name = @Name, description = @Description, price = @Price, weight = @Weight, width = @Width, height = @Height, depth = @Depth, quantity = @Quantity, updated = @Updated
                    WHERE id = @Id", product);
                if (rowsAffected == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(product);
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
                // delete product from database
                var rowsAffected = await db.ExecuteAsync(@"UPDATE products SET deleted = @date WHERE id = @id", new { date = DateTime.UtcNow, id });
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
