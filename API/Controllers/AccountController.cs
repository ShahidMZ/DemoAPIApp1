﻿using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext context;
    private readonly ITokenService tokenService;
    private readonly IMapper mapper;

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        this.context = context;
        this.tokenService = tokenService;
        this.mapper = mapper;
    }

    // Task<ActionResult<>> is required if returning HTTP messages like BadRequest or Unauthorized. For other async methods, Task<> will suffice.
    [HttpPost("register")] // POST: /api/account/register
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {
        // Check if username already exists
        if (await UserExists(registerDTO.Username))
        {
            return BadRequest("Username is taken");
        }

        // Map the fields of RegisterDTO to a new AppUser
        AppUser user = this.mapper.Map<AppUser>(registerDTO);

        // The "using" directive can be used to dispose the object of an IDisposable class automatically. HMACSHA512 implements the IDisposable interface.
        using var hmac = new HMACSHA512();
        
        // Update the AppUser with the username and password hash and salt.
        user.UserName = registerDTO.Username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
        user.PasswordSalt = hmac.Key;

        // Add the user to the db.
        this.context.Users.Add(user);
        var result = await this.context.SaveChangesAsync();

        return new UserDTO
        {
            Username = user.UserName,
            Token = this.tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")] // POST: /api/account/login
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        // Can use Users.SingleOrDefaultAsync to get the user with the unique username from db 
        // or can use Users.FirstOrDefaultAsync to get the first user with the username.
        var user = await this.context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == loginDTO.Username);

        // Check if username exists in the db.
        if (user == null)
        {
            return Unauthorized("Invalid Username");
        }

        // Create an hmac variable using the Password Salt stored in the db for the user.
        using var hmac = new HMACSHA512(user.PasswordSalt);

        // Compute the hash of the input password.
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        // Compare the hashes of the stored password and the input password.
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid Password");
            }
        }

        return new UserDTO
        {
            Username = user.UserName,
            Token = this.tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        // Checks if there is any user in the db with the same username and returns a boolean.
        return await this.context.Users.AnyAsync(user => user.UserName == username.ToLower());
    }
}
