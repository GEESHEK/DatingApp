using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //Api action has completed, get result context back
        var resultContext = await next();
        //check if user is Authenticated (maybe not necessary because the control already authenticates users
        if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        var username = resultContext.HttpContext.User.GetUsername();

        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByUsernameAsync(username);
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}