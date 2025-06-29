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
builder.Logging.AddDebug();

// --- START CONFIGURATION LOADING ---
// Încarcă configurația STRICT din variabilele de mediu setate în Cloud Run.
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
var firebaseApiKey = builder.Configuration["Firebase:ApiKey"];

// Verificare strictă. Aplicația va crăpa la pornire dacă variabilele nu sunt setate.
// Acest lucru te ajută să depanezi rapid, arătând eroarea în log-urile Cloud Run.
if (string.IsNullOrEmpty(firebaseProjectId) || string.IsNullOrEmpty(firebaseApiKey))
{
    throw new InvalidOperationException("EROARE CRITICĂ: Variabilele de mediu 'Firebase:ProjectId' sau 'Firebase:ApiKey' nu sunt configurate. Te rog, setează-le în setările serviciului Cloud Run.");
}

// --- START FIREBASE INITIALIZATION ---
// Inițializează Firebase Admin folosind contul de serviciu asociat cu instanța Cloud Run.
// NU mai citește din fișiere locale.
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.GetApplicationDefault()
});

// --- START SERVICE REGISTRATION ---
// Înregistrează serviciile Firebase folosind configurația încărcată.
builder.Services.AddSingleton<FirestoreDb>(provider => FirestoreDb.Create(firebaseProjectId));
builder.Services.AddSingleton(FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance); // Această linie este corectă.
builder.Services.AddSingleton<IFirebaseConfig>(new FirebaseConfig
{
    ApiKey = firebaseApiKey,
    ProjectId = firebaseProjectId
});

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IInfoUserService, InfoUserService>();
builder.Services.AddScoped<IInfoUserRepository, InfoUserRepository>();

builder.Services.AddScoped<IOrarUserRepository, OrarUserRepository>();
builder.Services.AddScoped<IOrarUserService, OrarUserService>();

builder.Services.AddScoped<IParitateSaptRepository, ParitateSaptRepository>();
builder.Services.AddScoped<IParitateSaptService, ParitateSaptService>();

builder.Services.AddScoped<IWorkingDaysRepository, WorkingDaysRepository>();
builder.Services.AddScoped<IWorkingDaysService, WorkingDaysService>();

builder.Services.AddScoped<IDeclaratieService, DeclaratieService>();

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
        policy.WithOrigins(
            "https://127.0.0.1:4200",
            "http://localhost:4200",
            "https://localhost:4200",
            "http://127.0.0.1:4200",
            "https://platacuora.web.app",
            "https://platacuora.firebaseapp.com"
        )
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

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(Int32.Parse(port));
});

var app = builder.Build();

app.UseCors("AllowAngularClient");

// OPTIONS middleware (bypass authentication for preflight requests)
app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

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