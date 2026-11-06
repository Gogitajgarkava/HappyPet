using System.Text;
using System.Text.Json.Serialization;
using HappyPet.Application.Interfaces.Airs;
using HappyPet.Application.Interfaces.Code;
using HappyPet.Application.Interfaces.Mail;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Application.Options;
using HappyPet.Application.Services;
using HappyPet.Infrastructure.Airs;
using HappyPet.Infrastructure.ExentialServices;
using HappyPet.Infrastructure.Persistence;
using HappyPet.Infrastructure.Persistence.Repositories;
using HappyPet.Infrastructure.Security;
using HappyPet.Infrastructure.Securuty;
using HappyPet.Infrastructure.Storage;
using HappyPet.WebAPI.ErrorHandling;
using HappyPet.WebAPI.HealthChecks;
using HappyPet.WebAPI.Models;
using HappyPet.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var reverseProxyEnabled = builder.Configuration.GetValue<bool>("ReverseProxy:Enabled");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "არასწორი მნიშვნელობა."
                        : error.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Code = "VALIDATION_ERROR",
            Message = "მოთხოვნის მონაცემები არასწორია.",
            TraceId = context.HttpContext.TraceIdentifier,
            Errors = errors
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "შეიყვანე JWT ტოკენი: Bearer {token}"
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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var loginPermitLimit = GetPositiveInt("RateLimiting:LoginPermitLimit", 10);
var loginWindowMinutes = GetPositiveInt("RateLimiting:LoginWindowMinutes", 5);
var otpPermitLimit = GetPositiveInt("RateLimiting:OtpPermitLimit", 10);
var otpWindowMinutes = GetPositiveInt("RateLimiting:OtpWindowMinutes", 10);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        var httpContext = context.HttpContext;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse
            {
                Code = "RATE_LIMIT_EXCEEDED",
                Message = "ძალიან ბევრი მოთხოვნა. გთხოვთ სცადოთ ცოტა ხანში.",
                TraceId = httpContext.TraceIdentifier
            },
            cancellationToken);
    };

    options.AddPolicy("auth-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginPermitLimit,
                Window = TimeSpan.FromMinutes(loginWindowMinutes),
                QueueLimit = 0
            }));

    options.AddPolicy("auth-otp", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = otpPermitLimit,
                Window = TimeSpan.FromMinutes(otpWindowMinutes),
                QueueLimit = 0
            }));
});

if (reverseProxyEnabled)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
{
    throw new InvalidOperationException("Jwt:Secret კონფიგურაცია აუცილებელია და უნდა იყოს მინიმუმ 32 სიმბოლო.");
}

if (jwtOptions.AccessTokenMinutes <= 0)
{
    throw new InvalidOperationException("Jwt:AccessTokenMinutes უნდა იყოს დადებითი რიცხვი.");
}

if (jwtOptions.RefreshTokenDays <= 0)
{
    throw new InvalidOperationException("Jwt:RefreshTokenDays უნდა იყოს დადებითი რიცხვი.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.RefreshTokenHashKey) || jwtOptions.RefreshTokenHashKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:RefreshTokenHashKey კონფიგურაცია აუცილებელია და უნდა იყოს მინიმუმ 32 სიმბოლო.");
}

builder.Services.AddDbContext<HappyPetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSecret = jwtOptions.Secret;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IOtpHasher, OtpHasher>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddSingleton<IFileStorageService, S3FileStorageService>();

builder.Services.Configure<AirsOptions>(builder.Configuration.GetSection("Airs"));
var airsBaseUrl = builder.Configuration["Airs:BaseUrl"];
if (string.IsNullOrWhiteSpace(airsBaseUrl))
{
    throw new InvalidOperationException("Airs:BaseUrl კონფიგურაცია აუცილებელია.");
}

var airsApiKey = builder.Configuration["Airs:ApiKey"];
var airsTimeoutSeconds = builder.Configuration.GetValue<int?>("Airs:TimeoutSeconds") ?? 10;
if (airsTimeoutSeconds <= 0)
{
    airsTimeoutSeconds = 10;
}

builder.Services.AddHttpClient<IAirsClient, AirsClient>(client =>
{
    client.BaseAddress = new Uri(airsBaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(airsTimeoutSeconds);

    if (!string.IsNullOrWhiteSpace(airsApiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", airsApiKey);
    }
});
builder.Services.AddScoped<IAirsService, AirsService>();

builder.Services.AddHttpClient(AirsHealthCheck.HttpClientName, client =>
{
    client.BaseAddress = new Uri(airsBaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(Math.Min(airsTimeoutSeconds, 5));

    if (!string.IsNullOrWhiteSpace(airsApiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", airsApiKey);
    }
});

var healthChecks = builder.Services.AddHealthChecks()
    .AddDbContextCheck<HappyPetDbContext>("database", tags: new[] { "ready" });
healthChecks.AddCheck<AirsHealthCheck>("airs", tags: new[] { "ready" });
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
builder.Services.AddScoped<ICodeGenerator, CodeGenerator>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddHostedService<RefreshTokenCleanupService>();


var app = builder.Build();

if (reverseProxyEnabled)
{
    app.UseForwardedHeaders();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!reverseProxyEnabled)
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

int GetPositiveInt(string key, int defaultValue)
{
    var value = builder.Configuration.GetValue<int?>(key);
    return value is > 0 ? value.Value : defaultValue;
}

static string GetClientKey(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

public partial class Program
{
}
