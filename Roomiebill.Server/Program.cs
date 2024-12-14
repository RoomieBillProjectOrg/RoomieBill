using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.BillingService;
using Roomiebill.Server.GroupService;
using Roomiebill.Server.UserService;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the connection string for SQL Server using the settings from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add other services like UserService, GroupService, and BillingService
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<BillingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
