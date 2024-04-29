using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(corsPolicyBuilder => 
    corsPolicyBuilder
        .AllowAnyHeader()
        .AllowAnyMethod()
        //without this we are going to get problems authenticating to server from client to server
        .AllowCredentials()
        .WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

//this will retrieve and serve the index.html from our wwwroot folder which is used by default
app.UseDefaultFiles();
//serves the content in the wwwroot
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "FallBack");

//gives us access to all the services inside this program class
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    //reseeds the database and creates it if it doesn't exist > we can drop db to reset it
    await context.Database.MigrateAsync();
    //clear out the connections table on restart or crashes etc >> SQL lite doesn't have Truncate method
    await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
    await Seed.SeedUser(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();