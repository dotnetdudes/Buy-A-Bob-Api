using Dotnetdudes.Buyabob.Api;
using Dotnetdudes.Buyabob.Api.Routes;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Data;
using System.Text;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
   .Enrich.FromLogContext()
   .WriteTo.Console()
   .CreateBootstrapLogger();

Log.Information("Starting Buy-A-Bob Api application");

// Create the builder and services
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
     .ReadFrom.Configuration(context.Configuration)
     .ReadFrom.Services(services)
     .Enrich.FromLogContext());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "Dotnetdudes.Buyabob.Api", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Bobrigins",
                      policy =>
                      {
                          policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:8080")
                          .WithOrigins(builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:8080")
                          .WithHeaders(builder.Configuration["Cors:AllowedHeaders"] ?? "*").WithMethods(builder.Configuration["Cors:AllowedMethods"] ?? "*");
                      });
});

// add authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        // keycloak client settings
        options.MetadataAddress = builder.Configuration["Authentication:Schemes:KeycloakAuthentication:Metadata"] ?? "https://identity.dotnetdudes.com/realms/dotnetdudes/.well-known/openid-configuration";
        options.Authority = builder.Configuration["Authentication:Schemes:KeycloakAuthentication:ServerRealm"] ?? "https://identity.dotnetdudes.com/realms/dotnetdudes";
        options.Audience = builder.Configuration["Authentication:Schemes:KeycloakAuthentication:ClientId"] ?? "buyabob-dev-web";
        // issuer
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:Schemes:KeycloakAuthentication:ServerRealm"] ?? "https://identity.dotnetdudes.com/realms/dotnetdudes";
        options.TokenValidationParameters.ValidAudience = builder.Configuration["Authentication:Schemes:KeycloakAuthentication:ClientId"] ?? "buyabob-dev-web";
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateLifetime = true;
    });

// add authorization
builder.Services.AddAuthorizationBuilder().AddPolicy("BobAdmin", policy => policy.RequireRole("admin"));

// add postgressql database connection
builder.Services.AddScoped<IDbConnection>(provider =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAntiforgery();

builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

string defaultSaveDirectory = "/uploads";
builder.Configuration.Bind("FileSaveOptions", new FileSaveOptions { SaveDirectory = defaultSaveDirectory });

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
        =>
    {
        var error = context?.Features?.Get<IExceptionHandlerFeature>()?.Error;
        if (error is not null)
        {
            Log.Error(error, "Unhandled exception");
        }
        await Results.Problem().ExecuteAsync(context!);
    })
    );

app.UseCors("Bobrigins");

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapGet("/api/antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions { HttpOnly = false });

    return Results.Ok();
}).WithTags("Anti Forgery"); // .RequireAuthorization();

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

public record FileSaveOptions
{
    public string? SaveDirectory { get; init; }
}
