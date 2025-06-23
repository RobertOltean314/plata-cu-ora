using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using PlataCuOra.Server.Infrastructure.Firebase;
using PlataCuOra.Server.Repository.Implementation;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Services.Implementation;
using PlataCuOra.Server.Services.Interfaces;
using System.IO;
using System;
using System.Text.Json;
using PlataCuOraApp.Server.Repository.Implementation;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Implementation;
using PlataCuOraApp.Server.Services.Interfaces;
using PlataCuOraApp.Server.Services;
using PlataCuOraApp.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Set up logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Load Firebase configuration
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"] ?? "platacuora";

// Method 1: Try to load API key from appsettings.json
var firebaseApiKey = builder.Configuration["Firebase:ApiKey"];

// Method 2: If not found in appsettings.json, try to load from file
if (string.IsNullOrEmpty(firebaseApiKey))
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogWarning("API Key not found in configuration, trying to load from file");

    string firebaseApiKeyFile = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "firebaseApiKey.json");

    if (File.Exists(firebaseApiKeyFile))
    {
        try
        {
            string apiKeyJson = File.ReadAllText(firebaseApiKeyFile);
            using (JsonDocument doc = JsonDocument.Parse(apiKeyJson))
            {
                JsonElement root = doc.RootElement;
                firebaseApiKey = root.GetProperty("apiKey").GetString();
                logger.LogInformation("API Key loaded from file successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load API key from file");
        }
    }
    else
    {
        logger.LogError("API key file not found at: {FilePath}", firebaseApiKeyFile);
    }
}

// Verify the API key is not empty
if (string.IsNullOrEmpty(firebaseApiKey))
{
    throw new InvalidOperationException("Firebase API Key is missing. Please check your configuration or key files.");
}

// Initialize Firebase Admin
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("Keys/firebaseKey.json")
});

// Set Environment variable for Firestore
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/firebaseKey.json");

// Configure services
builder.Services.AddSingleton<FirestoreDb>(provider => FirestoreDb.Create(firebaseProjectId));
builder.Services.AddSingleton(FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance);
builder.Services.AddSingleton<IFirebaseConfig>(new FirebaseConfig
{
    ApiKey = firebaseApiKey,
    ProjectId = firebaseProjectId
});

// Register repositories and services
builder.Services.AddScoped<IFirebaseAuthWrapper, FirebaseAuthWrapper>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IUserInformationService, UserInformationService>();
builder.Services.AddScoped<IUserInformationRepository, UserInformationRepository>();

builder.Services.AddScoped<IUserScheduleRepository, UserScheduleRepository>();
builder.Services.AddScoped<IUserScheduleService, UserScheduleService>();

builder.Services.AddScoped<IWeekParityRepository, WeekParityRepository>();
builder.Services.AddScoped<IWeekParityService, WeekParityService>();

builder.Services.AddScoped<IDeclarationService, DeclarationService>();


// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true
        };
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("https://127.0.0.1:4200", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure HttpClient
builder.Services.AddHttpClient();

// Configure Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAngularClient");
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("/index.html");

// Add a simple debug endpoint
app.MapGet("/debug/config", (IFirebaseConfig config) => new
{
    ApiKeyStatus = string.IsNullOrEmpty(config.ApiKey) ? "Missing" : "Present",
    ProjectId = config.ProjectId,
    ApiKeyPreview = !string.IsNullOrEmpty(config.ApiKey)
        ? $"{config.ApiKey.Substring(0, Math.Min(4, config.ApiKey.Length))}..."
        : null
});

app.Run();