using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserManagement.Middleware;
using UserManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserManagement API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddHealthChecks();

// Register user service
builder.Services.AddSingleton<IUserService, InMemoryUserService>();

// CORS policy — adjust allowed origins for your front-end/dev environment.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              // Replace with explicit origins for production: SetIsOriginAllowed(origin => new Uri(origin).Host == "your-frontend-host")
              .SetIsOriginAllowed(_ => true);
    });
});

// JWT authentication configuration
// NOTE: Set Jwt:Key, Jwt:Issuer and Jwt:Audience in configuration (secrets, environment variables)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceWithStrongKeyStoredInSecrets";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TechHive";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TechHiveClients";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline configuration — ensure correct order:
// 1) Error-handling middleware (first) — standardize and log unhandled exceptions.
// 2) Authentication middleware (next) — populate HttpContext.User for downstream components.
// 3) Logging middleware (last among these) — capture requests/responses with authenticated user context.

// 1) Global error handling first
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagement API v1"));
}
else
{
    // Keep the handler but our ErrorHandlingMiddleware will still standardize JSON error output.
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("DefaultCorsPolicy");

// 2) Authentication & Authorization (authentication must run before logging so logs include user info)
app.UseAuthentication();
app.UseAuthorization();

// 3) Request/response auditing — placed after authentication/authorization so logs include principal
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapControllers();

// Lightweight health endpoint
app.MapHealthChecks("/health").AllowAnonymous();

// Ensure error endpoint exists for production exception handler (kept as a fallback)
app.MapGet("/error", () => Results.Problem(detail: "An unexpected error occurred."))
   .ExcludeFromDescription();

app.Run();
