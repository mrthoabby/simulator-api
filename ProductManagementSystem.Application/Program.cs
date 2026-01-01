using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Bson;
using ProductManagementSystem.Application.Common;
using ProductManagementSystem.Application.AppEntities.Products.Mappings;
using ProductManagementSystem.Application.AppEntities.Products.Repository;
using ProductManagementSystem.Application.AppEntities.Products.Services;
using ProductManagementSystem.Application.AppEntities.Users.Mappings;
using ProductManagementSystem.Application.AppEntities.Users.Repository;
using ProductManagementSystem.Application.AppEntities.Users.Services;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Mappings;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Repository;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Services;
using ProductManagementSystem.Application.AppEntities.UserPlans.Repository;
using ProductManagementSystem.Application.AppEntities.UserPlans.Services;
using ProductManagementSystem.Infrastructure.Providers;
using ProductManagementSystem.Application.AppEntities.Auth.Mappings;
using ProductManagementSystem.Application.AppEntities.Auth.Services;
using ProductManagementSystem.Application.AppEntities.Auth.Repository;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.Mappings;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.Services;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.Repository;
using ProductManagementSystem.Application.Jobs;
using ProductManagementSystem.Application.Infrastructure.MongoDB;
using ProductManagementSystem.Application.Common.Middleware;
using Serilog;
using System.Text;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Services;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Repository;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Mappings;
using ProductManagementSystem.Application.AppEntities.Quotations.Services;
using ProductManagementSystem.Application.AppEntities.Quotations.Repository;
using ProductManagementSystem.Application.AppEntities.Quotations.Mappings;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.AppEntities.Concepts.Domain;
using ProductManagementSystem.Application.Common.AppEntities.Errors;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables();

    var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
    var mongoDatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
    var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    var passwordPepper = Environment.GetEnvironmentVariable("SECURITY_PASSWORD_PEPPER");

    if (!string.IsNullOrEmpty(mongoConnectionString))
    {
        builder.Configuration["MongoDbSettings:ConnectionString"] = mongoConnectionString;
    }
    if (!string.IsNullOrEmpty(mongoDatabaseName))
    {
        builder.Configuration["MongoDbSettings:DatabaseName"] = mongoDatabaseName;
    }
    if (!string.IsNullOrEmpty(jwtSecretKey))
    {
        builder.Configuration["JwtSettings:SecretKey"] = jwtSecretKey;
    }
    if (!string.IsNullOrEmpty(jwtIssuer))
    {
        builder.Configuration["JwtSettings:Issuer"] = jwtIssuer;
    }
    if (!string.IsNullOrEmpty(jwtAudience))
    {
        builder.Configuration["JwtSettings:Audience"] = jwtAudience;
    }
    if (!string.IsNullOrEmpty(passwordPepper))
    {
        builder.Configuration["SecuritySettings:PasswordPepper"] = passwordPepper;
    }
}

builder.Services.AddRequestTimeouts(configure =>
{
    configure.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(10),
        TimeoutStatusCode = 408,
        WriteTimeoutResponse = async (context) =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Request timeout",
                message = "The request took too long to process and was canceled.",
                statusCode = 408
            }));
        }
    };


    configure.AddPolicy("LongRunning", TimeSpan.FromMinutes(5));
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());


        options.JsonSerializerOptions.UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.AllowTrailingCommas = true;


        var resolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(static typeInfo =>
        {
            if (typeInfo.Kind == System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)
            {
                foreach (var property in typeInfo.Properties)
                {
                    property.IsRequired = false;
                }
            }
        });
        options.JsonSerializerOptions.TypeInfoResolver = resolver;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            GlobalExceptionHandlerMiddleware.HandleValidationErrors(context);
    });

// Register health checks so infrastructure probes can hit `/health`.
builder.Services.AddHealthChecks();

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Management System API",
        Version = "v1",
        Description = "API para el Sistema de Gestión Y Simulación de Precios y Rentabilidad de Productos"
    });

    // Configuración de seguridad JWT para Swagger (solo si auth está habilitada)
    if (builder.Environment.IsDevelopment())
    {
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Ejemplo: \"Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    }
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
if (mongoDbSettings != null)
{
    try
    {
        var client = new MongoClient(mongoDbSettings.ConnectionString);
        builder.Services.AddSingleton<IMongoClient>(client);

        Log.Information("Checking database connection...");
        client.GetDatabase("admin")
        .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1))
        .Wait();
        Log.Information("Database connected successfully");

        builder.Services.AddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to connect to the database. The system cannot start.");
        throw new ApplicationError("The database is not available. Please verify that MongoDB is running.", ex);
    }
}
else
{
    Log.Fatal("MongoDB configuration not found. The system cannot start.");
    throw new ApplicationError("MongoDB configuration not found in appsettings.json");
}

// Configure Security Settings
var securitySettings = builder.Configuration.GetSection("SecuritySettings").Get<SecuritySettings>();
if (securitySettings != null)
{
    builder.Services.AddSingleton(securitySettings);
}

// Configure Auth Settings
var authSettings = builder.Configuration.GetSection("AuthSettings").Get<AuthSettings>();
if (authSettings != null)
{
    builder.Services.AddSingleton(authSettings);
}

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    // Validate JWT configuration
    if (string.IsNullOrEmpty(jwtSettings.SecretKey))
    {
        Log.Fatal("JWT SecretKey is not configured. Set JWT_SECRET_KEY environment variable.");
        throw new ApplicationError("JWT SecretKey is required. Configure JWT_SECRET_KEY environment variable.");
    }
    
    if (string.IsNullOrEmpty(jwtSettings.Issuer))
    {
        Log.Fatal("JWT Issuer is not configured. Set JWT_ISSUER environment variable.");
        throw new ApplicationError("JWT Issuer is required. Configure JWT_ISSUER environment variable.");
    }

    builder.Services.AddSingleton(jwtSettings);

    // Parse multiple audiences from comma/semicolon separated string
    var validAudiences = jwtSettings.GetAudiences().ToList();
    
    if (validAudiences.Count == 0)
    {
        Log.Fatal("JWT Audience is not configured. Set JWT_AUDIENCE environment variable.");
        throw new ApplicationError("JWT Audience is required. Configure JWT_AUDIENCE environment variable (comma-separated list).");
    }
    
    Log.Information("JWT configured with {AudienceCount} valid audience(s): {Audiences}", 
        validAudiences.Count, 
        string.Join(", ", validAudiences));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                // Support multiple audiences (comma/semicolon separated in JWT_AUDIENCE secret)
                ValidAudiences = validAudiences,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

}

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(ProductMappingProfile), typeof(UserMappingProfile), typeof(SubscriptionMappingProfile), typeof(AuthMappingProfile), typeof(GlobalParametersMappingProfile), typeof(ConceptCodeMappingProfile), typeof(QuotationMappingProfile));

builder.Services.AddHostedService<PaymentJob>();

// Register Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, MongoProductRepository>();

// Register User Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();

// Register Subscription Services
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISubscriptionRepository, MongoSubscriptionRepository>();

// Register UserPlan Services
builder.Services.AddScoped<IUserPlanService, UserPlanService>();
builder.Services.AddScoped<IUserPlanRepository, MongoUserPlanRepository>();

// Register Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthTokenRepository, MongoAuthTokenRepository>();

// Register GlobalParameters Services
builder.Services.AddScoped<IGlobalParametersService, GlobalParametersService>();
builder.Services.AddScoped<IGlobalParametersRepository, GlobalParametersRepository>();

// Register ConceptCodes Services
builder.Services.AddScoped<IConceptCodeService, ConceptCodeService>();
builder.Services.AddScoped<IConceptCodeRepository, ConceptCodeRepository>();

// Register Quotation Services
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<IQuotationRepository, MongoQuotationRepository>();

// Register ConceptDomainRules
builder.Services.AddScoped<IConceptDomainRules, ConceptDomainRules>();

// Register Payment Provider
builder.Services.AddScoped<IPaymentProvider, MockPaymentProvider>();

var app = builder.Build();

// Configure MongoDB 
MongoDbConfiguration.Configure();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management System API v1");
        c.RoutePrefix = string.Empty; // Esto hace que Swagger esté disponible en la raíz "/"
    });
}

app.UseHttpsRedirection();

app.UseRequestTimeouts();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();

// Health check is deliberately mapped before controllers so infra probes are always allowed.
app.MapHealthChecks("/health");

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetService<ILogger<Program>>();
    logger?.LogInformation("Product Management System API started successfully");
    logger?.LogInformation("Database connected and operational");

    var addresses = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()?.Addresses;

    if (app.Environment.IsDevelopment() && addresses?.Any() == true)
    {
        foreach (var address in addresses)
        {
            logger?.LogInformation("API available at: {Address}", address);
        }
    }
    else if (!app.Environment.IsDevelopment())
    {
        logger?.LogInformation("Application running in Production mode");
    }
});

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
