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
                    CREATE TABLE IF NOT EXISTS Customers
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirstName VARCHAR(120) NOT NULL,
                        LastName VARCHAR(120) NOT NULL,
                        Email VARCHAR(120) NOT NULL,
                        Deleted DATETIME
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Addresses
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerId INTEGER NOT NULL,
                        Street VARCHAR(120) NOT NULL,
                        Suburb VARCHAR(120) NOT NULL,
                        City VARCHAR(120) NOT NULL,
                        State VARCHAR(120) NOT NULL,
                        Postcode CHAR(4) NOT NULL,
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
                        Name VARCHAR(120) NOT NULL,
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
                        CartId INTEGER NOT NULL,
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
                        Name VARCHAR(120) NOT NULL,
                        Deleted DATETIME
                    );");

                // ProductTag
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ProductTags
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductId INTEGER NOT NULL,
                        TagId INTEGER NOT NULL,
                        FOREIGN KEY(ProductId) REFERENCES Products(Id),
                        FOREIGN KEY(TagId) REFERENCES Tags(Id)
                    );");

                // ShippingAddress
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ShippingAddresses
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name VARCHAR(120) NOT NULL,
                        Deleted DATETIME
                    );");

                // ShippingType
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ShippingTypes
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name VARCHAR(120) NOT NULL,
                        Deleted DATETIME
                    );");

                // Tags
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Tags
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name VARCHAR(120) NOT NULL,
                        Deleted DATETIME
                    );");
            }
        }
    }
}
