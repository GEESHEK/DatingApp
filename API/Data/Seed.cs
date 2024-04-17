using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUser(UserManager<AppUser> userManager)
    {
        //check if our db has data in it, don't seed if we already do
        if (await userManager.Users.AnyAsync()) return;
        //read from the Json file
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        //add this an lower case property names in the json will also work i.e. key 
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //deserialize json to a c# object
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
        //generate passwords
        foreach (var user in users)
        {
            user.UserName = user.UserName.ToLower();
            //this will create and save in db
            await userManager.CreateAsync(user, "Pa$$w0rd");
        }
    }
}