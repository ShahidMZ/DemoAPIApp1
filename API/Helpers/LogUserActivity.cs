using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // resultContext is of type ActionExecutedContext, which gives the context after the API action has been completed.
        // The ActionExecutingContext context variable gives the context before the API action has begun.
        var resultContext = await next();

        // Check if the user is authenticated.
        if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        // Get the user from the user repository using the ID in the HTTP token.
        // Update their LastActive property.
        // Then update the database.
        var userId = resultContext.HttpContext.User.GetUserId();
        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByIdAsync(userId);
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
