using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpLogging;
using System.Security.Claims;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using CVGeneratorAPI.Settings;
using CVGeneratorAPI.Services;
using CVGeneratorAPI.Services.Llm;

var builder = WebApplication.CreateBuilder(args);

// ===== Load LLM API key from secret file =====
var llmApiKeyFile = builder.Configuration["Llm:ApiKeyFile"];
if (!string.IsNullOrEmpty(llmApiKeyFile) && File.Exists(llmApiKeyFile))
{
    var llmSecret = File.ReadAllText(llmApiKeyFile).Trim();
    builder.Configuration["Llm:ApiKey"] = llmSecret;
}
// ===== Settings =====

// Bind MongoDB configuration section to <see cref="MongoDBSettings"/> 
// and register it for dependency injection.

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// ===== Load JWT secret from secret file =====
var jwtSecretFile = builder.Configuration["Jwt:SecretFile"];
if (!string.IsNullOrEmpty(jwtSecretFile) && File.Exists(jwtSecretFile))
{
    var jwtSecret = File.ReadAllText(jwtSecretFile).Trim();
    builder.Configuration["Jwt:Secret"] = jwtSecret;
}

// Bind JWT configuration section to <see cref="JwtSettings"/> 
// and register it for dependency injection.

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Bind LLM configuration section to <see cref="LlmSettings"/> 
// and register it for dependency injection.
builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection("Llm"));

// ===== Services =====


// Register core application services as singletons:
// - <see cref="CVService"/> for CV operations
// - <see cref="UserService"/> for user management
// - <see cref="TokenService"/> for JWT token handling
// - <see cref="CVTemplateService"/> for managing CV templates

builder.Services.AddSingleton<CVService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<CVTemplateService>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

// Register LLM-related services:
// - <see cref="LlmClient"/> for making HTTP requests to the LLM API
builder.Services.AddTransient<LlmAuthHandler>();
builder.Services.AddScoped<LlmService>();

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
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        // Optional: add logging for authentication events
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var hasAuth = ctx.Request.Headers.ContainsKey("Authorization");
                var auth = hasAuth ? ctx.Request.Headers["Authorization"].ToString() : "(none)";
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtEvents");
                logger.LogInformation("OnMessageReceived: has Authorization? {HasAuth}", hasAuth);
                // logger.LogInformation("Authorization header: {Auth}", auth); // comment in if needed
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtEvents");
                logger.LogInformation("OnTokenValidated: subject={Sub}, nameid={NameId}",
                    ctx.Principal?.Identity?.Name,
                    ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtEvents");
                logger.LogWarning(ctx.Exception, "OnAuthenticationFailed: {Message}", ctx.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });
// ===== Additional Middleware Services =====
// Register <see cref="LlmClient"/> for making HTTP requests to the LLM API.
builder.Services.AddHttpClient<ILlmClient, LlmClient>((sp, http) =>
{
    var s = sp.GetRequiredService<IOptions<LlmSettings>>().Value;
    http.BaseAddress = new Uri(s.BaseUrl);
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    http.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<LlmAuthHandler>();
// Add authorization services to enforce role- or policy-based access control.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});
// add HTTP request logging
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders
                    | HttpLoggingFields.ResponsePropertiesAndHeaders;
    // o.RequestHeaders.Add("Authorization"); 
});
// Add CORS policy to allow requests from any origin, which is useful for development.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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

// seed admin before handling requests
// using (var scope = app.Services.CreateScope())
// {
//     var users = scope.ServiceProvider.GetRequiredService<UserService>();
//     await users.EnsureAdminUserAsync();
// }

app.UseHttpsRedirection();
app.UseHttpLogging();   // logs method/path/status, headers, etc.
app.UseCors("Frontend");
// Auth order matters: authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
