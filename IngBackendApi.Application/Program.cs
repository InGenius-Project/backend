using System.Text;
using System.Text.Json.Serialization;
using AutoMapper.EquivalencyExpression;
using AutoWrapper;
using Hangfire;
using IngBackend.Repository;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Context;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Profiles;
using IngBackendApi.Services;
using IngBackendApi.Services.AreaService;
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

try
{
    builder.Configuration.AddJsonFile("appsettings.Secrets.json");
}
catch
{
    throw new SystemInitException("Secret File Not Found.");
}

var env = builder.Environment;
var config = builder.Configuration;

// Development
if (env.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json");
}

// Production
if (env.IsProduction())
{
    builder.Configuration.AddJsonFile("appsettings.Production.json");
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
    builder.Services.AddDbContext<IngDbContext>(options => options.UseSqlServer(connectionString));
}

// Add services to the container.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IService<,,>), typeof(Service<,,>)); // Repository Wrapper
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IAreaTypeService, AreaTypeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();

// builder.Services.AddScoped<ApiResponseMiddleware>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddControllers();
builder.Services.AddScoped<EmailService>();

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
        }
    );

// CORS
var devCorsPolicy = "_devCorsPolicy";
builder.Services.AddCors(options =>
    options.AddPolicy(
        name: devCorsPolicy,
        policy =>
        {
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

app.UseHangfireDashboard("/hangfire");
app.UseCors(devCorsPolicy);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure wwwroot/images/* directory exists, if not create it
var paths = config.GetSection("ImageSavePath").Get<Dictionary<string, string>>() ?? [];
foreach (var path in paths)
{
    var imagePath = Path.Combine(env.WebRootPath, path.Value);
    if (!Directory.Exists(imagePath))
    {
        Directory.CreateDirectory(imagePath);
    }
}

app.Run();
