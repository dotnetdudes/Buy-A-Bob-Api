using Dapper;
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
                    CREATE TABLE IF NOT EXISTS customers
                    (
                        id SERIAL PRIMARY KEY,
                        identifier VARCHAR(120) NOT NULL,
                        firstname VARCHAR(120) NOT NULL,
                        LastName VARCHAR(120) NOT NULL,
                        email VARCHAR(120) NOT NULL,
                        created TIMESTAMP default (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS addresses
                    (
                        id SERIAL PRIMARY KEY,
                        customerid INTEGER NOT NULL,
                        street VARCHAR(120) NOT NULL,
                        suburb VARCHAR(120) NOT NULL,
                        city VARCHAR(120) NOT NULL,
                        state VARCHAR(120) NOT NULL,
                        postcode CHAR(4) NOT NULL,
                        country VARCHAR(120) NOT NULL,
                        created TIMESTAMP default (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP
                    );
                ");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS categories
                    (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(120) NOT NULL,
                        created TIMESTAMP default (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS products
                    (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(120) NOT NULL,
                        price INTEGER NOT NULL,
                        description TEXT NOT NULL,
                        imageurl VARCHAR(120) NOT NULL,
                        weight NUMERIC(6,2) NOT NULL,
                        width NUMERIC(6,2) NOT NULL,
                        depth NUMERIC(6,2) NOT NULL,
                        height NUMERIC(6,2) NOT NULL,
                        quantity INTEGER NOT NULL,
                        created TIMESTAMP default (timezone('utc', now())),
                        updated TIMESTAMP,
                        issold BOOLEAN NOT NULL DEFAULT FALSE,
                        solddate TIMESTAMP,
                        deleted TIMESTAMP
                    );");

                // ProductCategory
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS productcategories
                    (
                        id SERIAL PRIMARY KEY,
                        productid INTEGER NOT NULL,
                        categoryid INTEGER NOT NULL,
                        created TIMESTAMP default (timezone('utc', now())),
                        deleted TIMESTAMP,
                        FOREIGN KEY(productid) REFERENCES Products(id) ON DELETE CASCADE,
                        FOREIGN KEY(categoryid) REFERENCES Categories(id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS tags
                    (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(120) NOT NULL,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS producttags
                    (
                        id SERIAL PRIMARY KEY,
                        productid INTEGER NOT NULL,
                        tagid INTEGER NOT NULL,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP,
                        FOREIGN KEY(productid) REFERENCES products(id) ON DELETE CASCADE,
                        FOREIGN KEY(tagid) REFERENCES tags(id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS statuses
                    (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(120) NOT NULL,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS shippingaddresses
                    (
                        id SERIAL PRIMARY KEY,
                        customerid INTEGER NOT NULL,
                        addressid INTEGER NOT NULL,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP,
                        FOREIGN KEY(customerid) REFERENCES customers(id) ON DELETE CASCADE,
                        FOREIGN KEY(addressid) REFERENCES addresses(id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS shippingtypes
                    (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(120) NOT NULL,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS carts
                    (
                        id SERIAL PRIMARY KEY,
                        customerid INTEGER NOT NULL,
                        statusid INTEGER,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP,
                        FOREIGN KEY(customerid) REFERENCES customers(id) ON DELETE CASCADE,
                        FOREIGN KEY(statusid) REFERENCES statuses(id)
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS cartitems
                    (
                        id SERIAL PRIMARY KEY,
                        cartid INTEGER NOT NULL,
                        productid INTEGER NOT NULL,
                        quantity INTEGER NOT NULL,
                        created TIMESTAMP DEFAULT (timezone('utc', now())),
                        updated TIMESTAMP,
                        deleted TIMESTAMP,
                        FOREIGN KEY(productid) REFERENCES products(id) ON DELETE CASCADE,
                        FOREIGN KEY(cartid) REFERENCES carts(id) ON DELETE CASCADE
                    );");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS orders
                    (
                        id SERIAL PRIMARY KEY,
                        cartid INTEGER NOT NULL,
                        statusid INTEGER NOT NULL,
                        subtotal NUMERIC(10,2) NOT NULL,
                        tax NUMERIC(10,2) NOT NULL,
                        shippingtypeid INTEGER NOT NULL,
                        shipping NUMERIC(10,2) NOT NULL,
                        shippingaddressid INTEGER NOT NULL,
                        contactname VARCHAR(120),
                        contactphone VARCHAR(120),
                        total NUMERIC(10,2) NOT NULL,
                        datepurchased TIMESTAMP DEFAULT (timezone('utc', now())),
                        dateshipped TIMESTAMP,
                        deleted TIMESTAMP,
                        FOREIGN KEY(cartid) REFERENCES carts(id) ON DELETE CASCADE,
                        FOREIGN KEY(statusid) REFERENCES statuses(id) ON DELETE NO ACTION,
                        FOREIGN KEY(shippingtypeid) REFERENCES shippingtypes(id) ON DELETE NO ACTION,
                        FOREIGN KEY(shippingaddressid) REFERENCES shippingaddresses(id) ON DELETE NO ACTION
                    );");

                // Seed Data
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM categories") == 0)
                {
                    db.Execute(@"
                            INSERT INTO categories (name, created) VALUES ('Paintings', '2020-01-01');
                            ");
                }

                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM statuses") == 0)
                {
                    db.Execute(@"
                            INSERT INTO statuses (name, created) VALUES ('Pending', '2020-01-01');
                            INSERT INTO statuses (name, created) VALUES ('Processing', '2020-01-01');
                            INSERT INTO statuses (name, created) VALUES ('Shipped', '2020-01-01');
                            INSERT INTO statuses (name, created) VALUES ('Delivered', '2020-01-01');
                            INSERT INTO statuses (name, created) VALUES ('Cancelled', '2020-01-01');
                            ");
                }

                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM shippingtypes") == 0)
                {
                    db.Execute(@"
                            INSERT INTO shippingtypes (name, created) VALUES ('Standard', '2020-01-01');
                            INSERT INTO shippingtypes (name, created) VALUES ('Express', '2020-01-01');
                            ");
                }

                // customers
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM customers") == 0)
                {
                    db.Execute(@"
                            INSERT INTO customers (identifier, firstname, lastname, email, created) VALUES ('1234567890', 'John', 'Morton', 'john@dotnetdudes.com', '2020-01-01'); 
                            ");
                }

                // products
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM products") == 0)
                {
                    db.Execute(@"
                            INSERT INTO products (name, price, description, imageurl, weight, width, depth, height, quantity, created, updated, issold) VALUES ('Painting 1', 100, 'Painting 1 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            INSERT INTO products (name, price, description, imageurl, weight, width, depth, height, quantity, created, updated, issold) VALUES ('Painting 2', 200, 'Painting 2 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            INSERT INTO products (name, price, description, imageurl, weight, width, depth, height, quantity, created, updated, issold) VALUES ('Painting 3', 300, 'Painting 3 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            INSERT INTO products (name, price, description, imageurl, weight, width, depth, height, quantity, created, updated, issold) VALUES ('Painting 4', 400, 'Painting 4 Description', 'https://picsum.photos/200/300', 1.2, 1.2, 1.2, 1.2, 1, '2020-01-01', '2020-01-01', false);
                            ");
                }

                // product categories
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM productcategories") == 0)
                {
                    db.Execute(@"
                            INSERT INTO productcategories (productid, categoryid, created) values (1, 1, '2020-01-01');
                            insert into productcategories (productid, categoryid, created) VALUES (2, 1, '2020-01-01');
                            INSERT INTO productcategories (productid, categoryid, created) VALUES (3, 1, '2020-01-01');
                            INSERT INTO productcategories (productid, categoryid, created) VALUES (4, 1, '2020-01-01');
                            ");
                }

                // tags
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM tags") == 0)
                {
                    db.Execute(@"
                            INSERT INTO tags (name, created) VALUES ('Tag 1', '2020-01-01');
                            INSERT INTO tags (name, created) VALUES ('Tag 2', '2020-01-01');
                            INSERT INTO tags (name, created) VALUES ('Tag 3', '2020-01-01');
                            INSERT INTO tags (name, created) VALUES ('Tag 4', '2020-01-01');
                            ");
                }

                // product tags
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM producttags") == 0)
                {
                    db.Execute(@"
                            insert into producttags (productid, tagid, created) VALUES (1, 1, '2020-01-01');
                            INSERT INTO producttags (productid, tagid, created) VALUES (2, 2, '2020-01-01');
                            INSERT INTO producttags (productid, tagid, created) VALUES (3, 3, '2020-01-01');
                            INSERT INTO producttags (productid, tagid, created) VALUES (4, 4, '2020-01-01');
                            ");
                }

                // addresses
                if (db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM addresses") == 0)
                {
                    db.Execute(@"
                            INSERT INTO addresses (customerid, street, suburb, city, state, postcode, country, created) VALUES (1, '123 Fake Street', 'Fake Suburb', 'Fake City', 'Fake State', '1234', 'Fake Country', '2020-01-01');
                            ");
                }
            }
        }
    }
}
