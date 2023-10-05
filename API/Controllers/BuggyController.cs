﻿using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController : BaseApiController
{
    private readonly DataContext context;

    public BuggyController(DataContext context)
    {
        this.context = context;
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return "secret text";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var user = this.context.Users.Find(-1);

        if (user == null) return NotFound();

        return user;
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
        var user = this.context.Users.Find(-1);

        var toReturn = user.ToString();

        return toReturn;
        
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("This was not a good request.");
    }
}
