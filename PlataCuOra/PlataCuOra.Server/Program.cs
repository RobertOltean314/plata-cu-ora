using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Microsoft.OpenApi.Models;
using PlataCuOra.Server.Repository.Implementation;
using PlataCuOra.Server.Repository.Interface;
using System.Text;
using System.Threading.Channels;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("Keys/firebaseKey.json")
});

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/firebaseKey.json");

builder.Services.AddSingleton<FirestoreDb>(provider =>
{
    return FirestoreDb.Create("platacuora");
});

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

builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(options =>
{
    
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors("AllowAngularClient");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");
app.UseStaticFiles();

app.Run();
