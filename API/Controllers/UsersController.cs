using System.Security.Claims;
using API.Controllers;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    // [AllowAnonymous]
    [HttpGet] // /api/users
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
    {
        return Ok(await this.userRepository.GetMembersAsync());

        // return Ok(await this.userRepository.GetUsersAsync());

        // var users = await this.userRepository.GetUsersAsync();
        // var usersToReturn = this.mapper.Map<IEnumerable<MemberDTO>>(users);
        
        // return Ok(usersToReturn);
    }

    // [HttpGet("{id}")] // /api/users/2
    // public async Task<ActionResult<MemberDTO>> GetUser(int id)
    // {
    //     var user = await this.userRepository.GetUserByIdAsync(id);
    //     var userToReturn = this.mapper.Map<MemberDTO>(user);

    //     return userToReturn;
    // }

    [HttpGet("{username}")] // /api/users/username
    public async Task<ActionResult<MemberDTO>> GetUser(string username)
    {
        return await this.userRepository.GetMemberAsync(username);
        // var user = await this.userRepository.GetUserByUsernameAsync(username);
        // var userToReturn = this.mapper.Map<MemberDTO>(user);

        // return userToReturn;
    }

    // Since the request is going to have a token as this is an authenticated request, we can get the username from the token itself.
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        // The controller has a User object available, which is a ClaimsPrincipal for the user associated with the executing action. This gives us access to the token and the claims within it.
        // Use the ? (optional) operator with FindFirst().Value method because it throws an ArgumentNullException if there is no claim in the token with a NameIdentifier.
        // Use the ClaimType.NameIdentifier to get the username since we used the JwtRegisteredClaimNames.NameId parameter to set the username in the TokenService.cs file.
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // When the user is retrieved from the user repository, Entity Framework starts tracking the user. Any updates to the user are going to be automatically tracked by the Framework.
        // Not adding await will throw a "Missing type map configuration or unsupported mapping" Automapper Exception.
        var user = await this.userRepository.GetUserByUsernameAsync(username);

        if (user == null) return NotFound();

        // The mapper maps the fields in the memberUpdateDTO to the corresponding fields in the user. The database is not updated at this point.
        this.mapper.Map(memberUpdateDTO, user);

        // Use the userRepository.SaveAllAsync() method to update the DB.
        // If the DB has been successfully updated, return NoContent(), which gives a 204 return code that says there are no more updates to be made.
        if (await this.userRepository.SaveAllAsync()) return NoContent();

        // If the DB has not been successfully updated or if there were no changes to be saved, return a BadRequest().
        return BadRequest("API ERROR: Failed to update the user.");
    }
}
