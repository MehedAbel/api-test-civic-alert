using System.Text;
using System.Text.Json.Serialization;
using CivicAlert.Api.Data;
using CivicAlert.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CivicAlert.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCivicAlertServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CivicAlertDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            }));

        services.AddScoped<JwtService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IssueService>();
        services.AddScoped<UserService>();
        services.AddScoped<StorageService>();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        var jwtSection = configuration.GetSection("JwtSettings");
        var secret = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            var envOrigins = (configuration["CORS_ALLOWED_ORIGINS"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var origins = configured
                .Concat(envOrigins)
                .Concat(["http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000"])
                .Select(o => o.TrimEnd('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            options.AddPolicy("CivicAlertCors", policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Civic Alert API",
                Version = "v1",
                Description = "ASP.NET Core .NET 10 Controller-based API with JWT Authentication for Civic Issue Reporting."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token obtained from /api/auth/login or /api/auth/register."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddHealthChecks();
        return services;
    }
}
