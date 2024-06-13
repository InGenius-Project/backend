using System.Text;
using System.Text.Json.Serialization;
using AutoMapper.EquivalencyExpression;
using AutoWrapper;
using Hangfire;
using Hangfire.Dashboard;
using IngBackend.Repository;
using IngBackendApi.Application.Hubs;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Context;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Profiles;
using IngBackendApi.Services;
using IngBackendApi.Services.AreaService;
using IngBackendApi.Services.Http;
using IngBackendApi.Services.RecruitmentService;
using IngBackendApi.Services.TagService;
using IngBackendApi.Services.TokenServices;
using IngBackendApi.Services.UnitOfWork;
using IngBackendApi.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


var env = builder.Environment;
var config = builder.Configuration;

// Development
if (env.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Secrets.json");
    builder.Configuration.AddJsonFile("appsettings.Development.json");
}

// Production
if (env.IsProduction())
{
    builder.Configuration.AddJsonFile("appsettings.Production.json").AddEnvironmentVariables();
    var portVar = Environment.GetEnvironmentVariable("PORT");
    if (portVar is { Length: > 0 } && int.TryParse(portVar, out var port))
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port);
        });
    }
}

// Connnect to database
var connectionString = builder.Configuration.GetConnectionString("Default");

if (env.IsDevelopment())
{
    builder.Services.AddDbContext<IngDbContext>(options =>
    {
        options.UseSqlite(
            connectionString,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        );
        options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

        options.EnableSensitiveDataLogging();
    });
}
else
{
    builder.Services.AddDbContext<IngDbContext>(options =>
        options.UseSqlServer(
            connectionString,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        )
    );
}

// Add services to the container.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IService<,,>), typeof(Service<,,>));
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>(); // Repository Wrapper
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IAreaTypeService, AreaTypeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
builder.Services.AddSingleton<IGroupMapService, GroupMapService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<AiHttpClient>();
builder.Services.AddSingleton<UnsplashHttpClient>();
builder.Services.AddSingleton<ISettingsFactory, SettingsFactory>();

builder.Services.AddControllers();

// Add SignalR
builder
    .Services.AddSignalR()
    .AddJsonProtocol(option => option.PayloadSerializerOptions.PropertyNamingPolicy = null);

// Add Claim Accessor for SignalR
builder.Services.AddHttpContextAccessor();

// Json Serializer
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Add Logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        }
    );
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
        }
    );
});

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddCollectionMappers();
    cfg.AddProfile(
        new MappingProfile(builder.Services.BuildServiceProvider().GetService<IPasswordHasher>())
    );
    cfg.AddProfile(
        new MappingProfile(builder.Services.BuildServiceProvider().GetService<IConfiguration>())
    );
});

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Add SignalR authentication verify
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };

        // Add normal controller authentication
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Secrets:JwtSecretKey"])
            )
        };
    });

// CORS
var devCorsPolicy = "_devCorsPolicy";
builder.Services.AddCors(options =>
    options.AddPolicy(
        name: devCorsPolicy,
        policy =>
        {
            policy.WithOrigins("*").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
            policy
                .WithOrigins("https://ingenius.website", "https://www.ingenius.website")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
            policy
                .WithOrigins("https://150.117.18.40")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
            policy
                .WithOrigins("http://localhost:34004")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            policy
                .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
            policy
                .WithOrigins("http://140.123.176.230:34004")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    )
);

// Hangfire (Memory Storage)
builder.Services.AddHangfire(config => config.UseInMemoryStorage());
builder.Services.AddHangfireServer();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
// app.UseMiddleware<ApiResponseMiddleware>();
app.UseApiResponseAndExceptionWrapper(
    new AutoWrapperOptions
    {
        // UseApiProblemDetailsException = true,
        ExcludePaths = [new AutoWrapperExcludePath("/hangfire", ExcludeMode.StartWith)],
        ShowIsErrorFlagForSuccessfulResponse = true,
        ShowStatusCode = true,
    }
);

app.UseHangfireDashboard(
    "/hangfire",
    new DashboardOptions() { Authorization = [new LocalRequestsOnlyAuthorizationFilter()] }
);
app.UseCors(devCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/Chat");

// Ensure wwwroot/images/* directory exists, if not create it
var paths = config.GetSection("Path").GetSection("Image").Get<Dictionary<string, string>>() ?? [];
foreach (var path in paths)
{
    var imagePath = Path.Combine(env.WebRootPath, path.Value);
    if (!Directory.Exists(imagePath))
    {
        Directory.CreateDirectory(imagePath);
    }
}

app.Run();
