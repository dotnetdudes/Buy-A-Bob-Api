using Dotnetdudes.Buyabob.Api;
using Dotnetdudes.Buyabob.Api.Routes;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Data;

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
   .Enrich.FromLogContext()
   .WriteTo.Console()
   .CreateBootstrapLogger();

Log.Information("Starting Buy-A-Bob Api application");
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
     .ReadFrom.Configuration(context.Configuration)
     .ReadFrom.Services(services)
     .Enrich.FromLogContext());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Bobrigins",
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:8080")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// add authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        // keycloak client settings
        options.MetadataAddress = "https://identity.dotnetdudes.com/auth/realms/dotnetdudes/.well-known/openid-configuration";
        options.Authority = "https://identity.dotnetdudes.com/realms/dotnetdudes";
        options.Audience = "buyabob-dev-web";
    });

// add postgressql database connection
builder.Services.AddScoped<IDbConnection>(provider =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    try
    {
        DbInitialiser.Initialise(app);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error initialising database");
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages(async statusCodeContext 
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeContext.HttpContext));

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => {
            var error = context?.Features?.Get<IExceptionHandlerFeature>()?.Error;
            if(error is not null)
            {
                Log.Error(error, "Unhandled exception");
            }            
            await Results.Problem().ExecuteAsync(context!);
        })
    ) ;    

app.UseCors("Bobrigins");           

app.MapGroup("/api/customers").MapCustomerEndpoints().WithTags("Customers");
app.MapGroup("/api/address").MapAddressEndpoints().WithTags("Address");
app.MapGroup("/api/products").MapProductEndpoints().WithTags("Products");
app.MapGroup("/api/category").MapCategoryEndpoints().WithTags("Category");
app.MapGroup("/api/productcategory").MapProductCategoryEndpoints().WithTags("Product Category");
app.MapGroup("/api/tags").MapTagEndpoints().WithTags("Tags");
app.MapGroup("/api/producttags").MapProductTagEndpoints().WithTags("Product Tags");
app.MapGroup("/api/status").MapStatusEndpoints().WithTags("Status");
app.MapGroup("/api/cart").MapCartEndpoints().WithTags("Cart");
app.MapGroup("/api/cartitem").MapCartItemEndpoints().WithTags("Cart Item");
app.MapGroup("/api/orders").MapOrderEndpoints().WithTags("Orders");
app.MapGroup("/api/ShippingAddress").MapShippingAddressEndpoints().WithTags("Shipping Address");
app.MapGroup("/api/shippingtype").MapShippingTypeEndpoints().WithTags("Shipping Type");

app.Run();

public partial class Program { }
