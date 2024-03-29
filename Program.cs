using System.Text;
using AutoMapper.EntityFramework;
using AutoMapper.EquivalencyExpression;
using AutoWrapper;
using Hangfire;
using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.Service;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Profiles;
using IngBackend.Repository;
using IngBackend.Services;
using IngBackend.Services.AreaService;
using IngBackend.Services.RecruitmentService;
using IngBackend.Services.TagService;
using IngBackend.Services.TokenServices;
using IngBackend.Services.UnitOfWork;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Secrets.json");

IWebHostEnvironment env = builder.Environment;

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
string? connectionString = builder.Configuration.GetConnectionString("Default");

if (env.IsDevelopment())
{
    builder.Services.AddDbContext<IngDbContext>(options =>
    {
        options.UseSqlite(connectionString);
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
builder.Services.AddScoped<ResumeService>();
builder.Services.AddScoped<AreaService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<RecruitmentService>();

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
                new string[] { }
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
});

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
{
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
    );
});

// Hangfire (Memory Storage)
builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage();
});
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
        ShowIsErrorFlagForSuccessfulResponse = true,
        ShowStatusCode = true,
    }
);

app.UseCors(devCorsPolicy);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply Migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    Console.WriteLine("READY To Applying Migrations");

    var context = services.GetRequiredService<IngDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        Console.WriteLine("Applying Migrations...");
        context.Database.Migrate();
    }
}

app.Run();
