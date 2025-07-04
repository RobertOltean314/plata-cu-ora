﻿using FirebaseAdmin;
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
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("ro-RO");
cultureInfo.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


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
    Credential = GoogleCredential.GetApplicationDefault(),
    ProjectId = firebaseProjectId
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

builder.Services.AddHttpClient();
builder.Services.AddScoped<IHolidaysService, HolidaysService>();

// Configure JWT authentication
// Configure JWT authentication - FORMA CORECTĂ PENTRU FIREBASE
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Authority este SINGURA proprietate de care ai nevoie la acest nivel.
        // Ea va seta corect emitentul (issuer) și va găsi automat cheile publice de la Google.
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            // Validează că token-ul a fost emis de proiectul tău Firebase.
            ValidateIssuer = true,

            // Validează că token-ul este destinat pentru API-ul tău (audiența este ID-ul proiectului).
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,

            // Validează că token-ul nu a expirat.
            ValidateLifetime = true,

            // Foarte important: Asigură-te că semnătura este validată folosind cheile publice.
            ValidateIssuerSigningKey = true
        };
    });

// Configure CORS
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins(
            "https://platacuora.web.app",
            "https://platacuora.firebaseapp.com",
            "http://localhost:4200"
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

// UseCors trebuie să fie înainte de Authentication
app.UseCors("AllowAngularClient");

// Swagger + OPTIONS preflight
app.UseSwagger();
app.UseSwaggerUI();

// Middleware ca să răspundă la OPTIONS (preflight) fără 401
app.Use(async (context, next) =>
{
    if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Swagger root redirect
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