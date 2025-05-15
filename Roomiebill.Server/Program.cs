using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Services;
using Roomiebill.Server.Models;
using Roomiebill.Server.Facades;
using Microsoft.AspNetCore.Identity;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Net;
using Roomiebill.Server.Common;
using Roomiebill.Server.Services.Interfaces;
using Roomiebill.Server.Facades;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuck
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Application Insights
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

// Configure the connection string for SQL Server using the settings from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InbarLocalConnection")));
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();


// Register configuration and singletons first
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<GeminiService>();

// Register core services
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register facades
builder.Services.AddScoped<UserFacade>(provider => {
    var dbContext = provider.GetRequiredService<IApplicationDbContext>();
    var passwordHasher = provider.GetRequiredService<IPasswordHasher<User>>();
    var logger = provider.GetRequiredService<ILogger<UserFacade>>();
    return new UserFacade(dbContext, passwordHasher, logger);
});

// Register services in dependency order
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IGroupInviteMediatorService, GroupInviteMediatorService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IUserFacade, UserFacade>();

// Register database seeder last since it depends on other services
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddHostedService<PaymentReminderService>();
builder.Services.Configure<HostOptions>(opts => 
{
    opts.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("private_key.json")
});

// Apply migrations and ensure database creation
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    // Seed the database
    var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await databaseSeeder.SeedAsync();
}

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
