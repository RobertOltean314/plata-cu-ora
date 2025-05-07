using Google;
using PlataCuOra.Server.Data;
using PlataCuOra.Server.Repository.Implementation;
using PlataCuOra.Server.Repository.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddFirestoreDb(options =>
//{
//	options.CredentialsPath = "firebaseKey.json";
//	options.ProjectId = "PlataCuOra";
//});

//builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(options => {
	options.AllowAnyHeader();
	options.AllowAnyOrigin();
	options.AllowAnyMethod();
});


app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
