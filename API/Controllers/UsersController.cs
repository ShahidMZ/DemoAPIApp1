using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
