using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task ClearConnections(DataContext context)
    {
        context.Connections.RemoveRange(context.Connections);
        await context.SaveChangesAsync();
    }
    
    public static async Task SeedUser(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        //check if our db has data in it, don't seed if we already do
        if (await userManager.Users.AnyAsync()) return;
        //read from the Json file
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        //add this an lower case property names in the json will also work i.e. key 
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //deserialize json to a c# object
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
        
        //create some roles and ad to db
        var roles = new List<AppRole>()
        {
            new AppRole { Name = "Member" },
            new AppRole { Name = "Admin" },
            new AppRole { Name = "Moderator" },
        };
      
        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }
        
        //generate passwords
        foreach (var user in users)
        {
            user.UserName = user.UserName.ToLower();
            //UTC is need for postgres to work
            user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
            user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);
            //this will create and save in db
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        //add an admin with no other props
        var admin = new AppUser
        {
            UserName = "admin"
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        //can add more than one role to a user
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }
}