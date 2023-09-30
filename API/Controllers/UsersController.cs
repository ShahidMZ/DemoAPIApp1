using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controller;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly DataContext context;

    public UsersController(DataContext context)
    {
        this.context = context;
    }

    [AllowAnonymous]
    [HttpGet] // /api/users
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var userList = this.context.Users.ToListAsync();

        return await userList;
    }

    [HttpGet("{id}")] // /api/users/2
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        return await this.context.Users.FindAsync(id);
    }
}
