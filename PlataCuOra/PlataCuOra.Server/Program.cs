using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlataCuOra.Server.Data;
using PlataCuOra.Server.Repo.Implementation;
using PlataCuOra.Server.Repo.Interface;
using PlataCuOra.Server.Utils;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

PasswordHasher.SetPasswordKey(builder.Configuration["Keys:Password"]!);

builder.Services.AddScoped<IUserRepository, UserRepository>();

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
