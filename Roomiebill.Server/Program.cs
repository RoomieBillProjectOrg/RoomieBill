using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Services;
using Roomiebill.Server.Models;
using Microsoft.AspNetCore.Identity;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Net;
using Firebase.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuck
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Application Insights
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

// Configure the connection string for SQL Server using the settings from appsettings.json
// 🔴 REMOVE SQL SERVER IF SWITCHING TO FIREBASE ONLY 🔴
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
// ✅ Register Realtime Database Service (Preferred for Live Sync)

builder.Services.AddSingleton(provider => new FirebaseClient("https://roomiebill-34199-default-rtdb.europe-west1.firebasedatabase.app/"));

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("private_key.json")
});

// Add other services like UserService, GroupService, and BillingService
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<UserService>();
// builder.Services.AddScoped<GroupService>();
// builder.Services.AddScoped<InviteService>();
// builder.Services.AddScoped<GroupInviteMediatorService>();
// builder.Services.AddScoped<BillingService>();
// builder.Services.AddScoped<DatabaseSeeder>();
// builder.Services.AddScoped<IPaymentService, MockPaymentService>();

var app = builder.Build();



// 🔴 REMOVE EF Core MIGRATIONS IF NOT USING SQL SERVER 🔴
// Apply migrations and ensure database creation
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     dbContext.Database.Migrate();

//     // Seed the database
//     var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
//     await databaseSeeder.SeedAsync();
// }


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
