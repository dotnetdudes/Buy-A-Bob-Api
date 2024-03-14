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
                        id SERIAL PRIMARY KEY,
                        Identifier VARCHAR(120) NOT NULL,
                        FirstName VARCHAR(120) NOT NULL,
                        LastName VARCHAR(120) NOT NULL,
                        Email VARCHAR(120) NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Addresses
                    (
                        id SERIAL PRIMARY KEY,
                        CustomerId INTEGER NOT NULL,
                        Street VARCHAR(120) NOT NULL,
                        Suburb VARCHAR(120) NOT NULL,
                        City VARCHAR(120) NOT NULL,
                        State VARCHAR(120) NOT NULL,
                        Postcode CHAR(4) NOT NULL,
                        Country VARCHAR(120) NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP
                    );
                ");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Categories
                    (
                        id SERIAL PRIMARY KEY,
                        Name VARCHAR(120) NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Products
                    (
                        id SERIAL PRIMARY KEY,
                        Name VARCHAR(120) NOT NULL,
                        Price INTEGER NOT NULL,
                        Description TEXT NOT NULL,
                        ImageUrl VARCHAR(120) NOT NULL,
                        Weight NUMERIC(6,2) NOT NULL,
                        Width NUMERIC(6,2) NOT NULL,
                        Depth NUMERIC(6,2) NOT NULL,
                        Height NUMERIC(6,2) NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        IsSold BOOLEAN NOT NULL DEFAULT FALSE,
                        SoldDate TIMESTAMP,
                        Deleted TIMESTAMP
                    );");

                // ProductCategory
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ProductCategories
                    (
                        id SERIAL PRIMARY KEY,
                        ProductId INTEGER NOT NULL,
                        CategoryId INTEGER NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Deleted TIMESTAMP,
                        FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                        FOREIGN KEY(CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Tags
                    (
                        id SERIAL PRIMARY KEY,
                        Name VARCHAR(120) NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ProductTags
                    (
                        id SERIAL PRIMARY KEY,
                        ProductId INTEGER NOT NULL,
                        TagId INTEGER NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP,
                        FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                        FOREIGN KEY(TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Statuses
                    (
                        id SERIAL PRIMARY KEY,
                        Name VARCHAR(120) NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ShippingAddresses
                    (
                        id SERIAL PRIMARY KEY,
                        OrderId INTEGER NOT NULL,
                        AddressId INTEGER NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ShippingTypes
                    (
                        id SERIAL PRIMARY KEY,
                        Name VARCHAR(120) NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Carts
                    (
                        id SERIAL PRIMARY KEY,
                        CustomerId INTEGER NOT NULL,
                        StatusId INTEGER NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP,
                        FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
                        FOREIGN KEY(StatusId) REFERENCES Statuses(Id) ON DELETE NO ACTION
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS CartItems
                    (
                        id SERIAL PRIMARY KEY,
                        CartId INTEGER NOT NULL,
                        ProductId INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Created TIMESTAMP default (timezone('utc', now())),
                        Updated TIMESTAMP,
                        Deleted TIMESTAMP,
                        FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                        FOREIGN KEY(CartId) REFERENCES Carts(Id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Orders
                    (
                        id SERIAL PRIMARY KEY,
                        CartId INTEGER NOT NULL,
                        StatusId INTEGER NOT NULL,
                        SubTotal NUMERIC(10,2) NOT NULL,
                        Tax NUMERIC(10,2) NOT NULL,
                        ShippingTypeId INTEGER NOT NULL,
                        Shipping NUMERIC(10,2) NOT NULL,
                        ShippingAddressId INTEGER NOT NULL,
                        ContactName VARCHAR(120),
                        ContactPhone VARCHAR(120),
                        Total NUMERIC(10,2) NOT NULL,
                        DatePurchased TIMESTAMP default (timezone('utc', now())),
                        DateShipped TIMESTAMP,
                        Deleted TIMESTAMP,
                        FOREIGN KEY(CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
                        FOREIGN KEY(StatusId) REFERENCES Statuses(Id) ON DELETE NO ACTION,
                        FOREIGN KEY(ShippingTypeId) REFERENCES ShippingTypes(Id) ON DELETE NO ACTION,
                        FOREIGN KEY(ShippingAddressId) REFERENCES ShippingAddresses(Id) ON DELETE NO ACTION
                    );");

                // Seed Data
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Categories") == 0)
                {
                    db.Execute(@"
                            INSERT INTO Categories (Name, Created) VALUES ('Paintings', '2020-01-01');
                            ");
                }

                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Statuses") == 0)
                {
                    db.Execute(@"
                            INSERT INTO Statuses (Name, Created) VALUES ('Pending', '2020-01-01');
                            INSERT INTO Statuses (Name, Created) VALUES ('Processing', '2020-01-01');
                            INSERT INTO Statuses (Name, Created) VALUES ('Shipped', '2020-01-01');
                            INSERT INTO Statuses (Name, Created) VALUES ('Delivered', '2020-01-01');
                            INSERT INTO Statuses (Name, Created) VALUES ('Cancelled', '2020-01-01');
                            ");
                }

                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM ShippingTypes") == 0)
                {
                    db.Execute(@"
                            INSERT INTO ShippingTypes (Name, Created) VALUES ('Standard', '2020-01-01');
                            INSERT INTO ShippingTypes (Name, Created) VALUES ('Express', '2020-01-01');
                            ");
                }

                // customers
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Customers") == 0)
                {
                    db.Execute(@"
                            INSERT INTO Customers (Identifier, FirstName, LastName, Email, Created) VALUES ('1234567890', 'John', 'Morton', 'john@dotnetdudes.com', '2020-01-01'); 
                            ");
                }

                // products
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Products") == 0)
                {
                    db.Execute(@"
                            INSERT INTO Products (Name, Price, Description, ImageUrl, Weight, Width, Depth, Height, Quantity, Created, Updated, IsSold) VALUES ('Painting 1', 100, 'Painting 1 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            INSERT INTO Products (Name, Price, Description, ImageUrl, Weight, Width, Depth, Height, Quantity, Created, Updated, IsSold) VALUES ('Painting 2', 200, 'Painting 2 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            INSERT INTO Products (Name, Price, Description, ImageUrl, Weight, Width, Depth, Height, Quantity, Created, Updated, IsSold) VALUES ('Painting 3', 300, 'Painting 3 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            INSERT INTO Products (Name, Price, Description, ImageUrl, Weight, Width, Depth, Height, Quantity, Created, Updated, IsSold) VALUES ('Painting 4', 400, 'Painting 4 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            ");
                }

                // product categories
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM ProductCategories") == 0)
                {
                    db.Execute(@"
                            INSERT INTO ProductCategories (ProductId, CategoryId, Created) VALUES (1, 1, '2020-01-01');
                            INSERT INTO ProductCategories (ProductId, CategoryId, Created) VALUES (2, 1, '2020-01-01');
                            INSERT INTO ProductCategories (ProductId, CategoryId, Created) VALUES (3, 1, '2020-01-01');
                            INSERT INTO ProductCategories (ProductId, CategoryId, Created) VALUES (4, 1, '2020-01-01');
                            ");
                }

                // tags
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Tags") == 0)
                {
                    db.Execute(@"
                            INSERT INTO Tags (Name, Created) VALUES ('Tag 1', '2020-01-01');
                            INSERT INTO Tags (Name, Created) VALUES ('Tag 2', '2020-01-01');
                            INSERT INTO Tags (Name, Created) VALUES ('Tag 3', '2020-01-01');
                            INSERT INTO Tags (Name, Created) VALUES ('Tag 4', '2020-01-01');
                            ");
                }

                // product tags
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM ProductTags") == 0)
                {
                    db.Execute(@"
                            INSERT INTO ProductTags (ProductId, TagId, Created) VALUES (1, 1, '2020-01-01');
                            INSERT INTO ProductTags (ProductId, TagId, Created) VALUES (2, 2, '2020-01-01');
                            INSERT INTO ProductTags (ProductId, TagId, Created) VALUES (3, 3, '2020-01-01');
                            INSERT INTO ProductTags (ProductId, TagId, Created) VALUES (4, 4, '2020-01-01');
                            ");
                }

                // addresses
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Addresses") == 0)
                {
                    db.Execute(@"
                            INSERT INTO Addresses (CustomerId, Street, Suburb, City, State, Postcode, Country, Created) VALUES (1, '123 Fake Street', 'Fake Suburb', 'Fake City', 'Fake State', '1234', 'Fake Country', '2020-01-01');
                            ");
                }
            }
        }
    }
}
