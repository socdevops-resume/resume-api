using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CVGeneratorAPI.Settings;
using CVGeneratorAPI.Services;

var builder = WebApplication.CreateBuilder(args);


// ===== Settings =====

// Bind MongoDB configuration section to <see cref="MongoDBSettings"/> 
// and register it for dependency injection.

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));


// Bind JWT configuration section to <see cref="JwtSettings"/> 
// and register it for dependency injection.

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// ===== Services =====


// Register core application services as singletons:
// - <see cref="CVService"/> for CV operations
// - <see cref="UserService"/> for user management
// - <see cref="TokenService"/> for JWT token handling

builder.Services.AddSingleton<CVService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TokenService>();

// ===== Controllers =====
// Add controller support and minimal API endpoint discovery.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== Swagger (+ Bearer button) =====


// Configure Swagger with API metadata and JWT Bearer authentication support.
// Adds an "Authorize" button to Swagger UI for testing secured endpoints.

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CV Generator API",
        Version = "v1",
        Description = "Strict REST routes with JWT authentication"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ===== Authentication / Authorization =====

// Retrieve JWT settings from configuration and set up authentication using JWT Bearer tokens.
// Includes validation for issuer, audience, signing key, and token lifetime.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
          ?? throw new InvalidOperationException("Jwt settings are missing.");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });


// Add authorization services to enforce role- or policy-based access control.
builder.Services.AddAuthorization();

var app = builder.Build();

// ===== Pipeline =====


// Configure request pipeline:
// - Enable Swagger in development
// - Enforce HTTPS redirection
// - Enable authentication and authorization middleware
// - Map controllers to endpoints
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Auth order matters: authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
