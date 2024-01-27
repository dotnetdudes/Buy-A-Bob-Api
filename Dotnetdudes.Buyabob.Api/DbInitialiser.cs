using Dapper;
using Dotnetdudes.Buyabob.Api.Models;
using System.Data;

namespace Dotnetdudes.Buyabob.Api
{
    public class DbInitialiser
    {
        public static void Initialise(WebApplication app)
        { 
                       using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var env = services.GetRequiredService<IHostEnvironment>();
            if (env.IsDevelopment())
            {
                var db = services.GetRequiredService<IDbConnection>();
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Addresses
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Line1 TEXT NOT NULL,
                        Line2 TEXT,
                        Town TEXT NOT NULL,
                        County TEXT NOT NULL,
                        Postcode TEXT NOT NULL,
                        Deleted DATETIME
                    );
                ");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Orders
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AddressId INTEGER NOT NULL,
                        FOREIGN KEY(AddressId) REFERENCES Addresses(Id)
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Products
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Price INTEGER NOT NULL,
                        Deleted DATETIME
                    );");

                // cart
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS CartItems
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductId INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        CartId TEXT NOT NULL,
                        FOREIGN KEY(ProductId) REFERENCES Products(Id)
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Cart
                    (
                        Id TEXT PRIMARY KEY,
                        Created DATETIME NOT NULL,
                        Updated DATETIME NOT NULL
                    );");

                // Categories
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Categories
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Deleted DATETIME
                    );");

                // ProductCategories
                
            }
        }
    }
}
