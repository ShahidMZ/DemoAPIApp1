using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUserRepository userRepository;
    private readonly ILikesRepository likesRepository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
        this.userRepository = userRepository;
        this.likesRepository = likesRepository;
    }

    // Current user likes another user who is specified by the username parameter.
    // Update the Likes table in the DB.
    [HttpPost("{username}")]    // /api/likes/ben
    public async Task<ActionResult> AddLike(string username)
    {
        // Get the ID of the currently logged in user.
        int sourceUserId = User.GetUserId();

        // Get the target user and verify if such a user exists.
        AppUser targetUser = await this.userRepository.GetUserByUsernameAsync(username);
        if (targetUser == null) return NotFound();

        // Get the current user (sourceUser) from the likesRepository with the LikedUsers property eagerly loaded.
        AppUser sourceUser = await this.likesRepository.GetUserWithLikes(sourceUserId);
        // Verify if the current user isn't trying to like themselves.
        if (sourceUser.UserName == username) return BadRequest("API ERROR: You cannot like yourself.");

        // Check if the current user has already liked the specified user.
        UserLike userLike = await likesRepository.GetUserLike(sourceUserId, targetUser.Id);
        if (userLike != null) return BadRequest("API ERROR: You already like this user.");

        // Create a new UserLike object with the source and target IDs.
        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = targetUser.Id            
        };

        // Add the userLike object to the current user's LikedUsers list.
        sourceUser.LikedUsers.Add(userLike);

        // Update the DB.
        if (await this.userRepository.SaveAllAsync()) return Ok();

        return BadRequest("API ERROR: Failed to like user.");
    }

    // Get the list of users liked by the current user or the list of users that like the current user.
    [HttpGet]   // /api/likes?predicate=liked or predicate=likedBy
    // Since the parameter is an object and not a string, we need to specify [FromQuery] to let the API server know it has to fetch the parameters from the request query string.
    // [FromQuery] specifies that the userparams are located in the URL query.
    // [FromQuery, Required] will specify that the data field value is required.
    // Not passing any parameters in the URL will set the default values for pageNumber and pageSize, set in the PaginationParams class (which LikesParams inherits).
    public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();

        var users = await this.likesRepository.GetUserLikes(likesParams);

        // Since we need a pagination header, add it to the response.
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, 
            users.TotalCount, users.TotalPages));

        return Ok(users);
    }
}
