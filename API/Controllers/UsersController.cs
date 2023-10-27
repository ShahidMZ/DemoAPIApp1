using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controller;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;
    private readonly IPhotoService photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.photoService = photoService;
    }

    // [AllowAnonymous]
    [HttpGet] // /api/users?pageNumber=1&pageSize=10
    public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
    {
        // [FromQuery] specifies that the userparams are located in the URL query.
        // [FromQuery, Required] will specify that the data field value is required.
        // Not passing any parameters in the URL will set the default values for pageNumber and pageSize, set in the UserParams class.
        
        var currentUser = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());
        
        // Set the currently logged in user's username in the user params.
        userParams.CurrentUserName = currentUser.UserName;

        if (userParams.Gender.IsNullOrEmpty())
        {
            // Set the opposite gender of the current user in the user params.
            userParams.Gender = currentUser.Gender =="male" ? "female" : "male";
        }

        var users = await this.userRepository.GetMembersAsync(userParams);
        
        // Return the pagination information via the pagination header.
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        return Ok(users);

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
        // Implemented in Extensions.ClaimsPrincipalExtensions.cs
        var username = User.GetUsername();
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

    [HttpPost("add-photo")] // /api/users/add-photo
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        // Get user from the repository using the username from the claims principal.
        var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound("API ERROR: UsersController.AddPhoto(): User does not exist!");

        // Upload the photo and get the results from Cloudinary.
        var result = await photoService.AddPhotoAsync(file);
        
        if (result == null) return BadRequest("API ERROR: File is empty!");
        if (result.Error != null) return BadRequest("API ERROR: " + result.Error.Message);

        // Create a new Photo object using the results parameters.
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        // Set the photo as main if it is the user's first photo.
        if (user.Photos.Count == 0) photo.IsMain = true;

        // Add the photo to the user object
        user.Photos.Add(photo);

        // // Use the userRepository.SaveAllAsync() method to update the DB.
        // In REST APIs, an HTTPPost equest should ideally return a 201 Created response, with the location details of the created object in the response header.
        if (await this.userRepository.SaveAllAsync()) 
        {
            // CreatedAtAction returns a 201 Created response.
            // nameof(GetUser) assigns the location of the created header as the URL of the GetUser() method above, i.e, /api/users/username.
            // The second parameter is the username, which is passed as an object, since it is required by the GetUser() method.
            // The last parameter is the PhotoDTO object.
            // This essentially creates a header in the 201 response with the /api/users/username value while also returning the photoDTO object.
            return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, mapper.Map<PhotoDTO>(photo));
        }

        return BadRequest("API ERROR: Failed to upload the photo.");
    }

    // photoId is the ID of the photo that is to be set as main.
    [HttpPut("set-main-photo/{photoId}")]   // /api/users/set-main-photo/11
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        // Get the user from the user repository and get the photo with the corresponding photo ID.
        var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        // If the photo is already the main photo, throw a BadRequest error.
        if (photo.IsMain) return BadRequest("This is already your main photo");

        // Get the main photo, if any, from the user.
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

        // If there is a main photo already, set IsMain as false.
        if (currentMain != null) currentMain.IsMain = false;

        // Set the new photo as the main.
        photo.IsMain = true;

        // Save to DB.
        if (await this.userRepository.SaveAllAsync()) return NoContent();

        // If saving to db failed, throw a BadRequest error.
        return BadRequest("API ERROR: Problem setting main photo.");
    }

    [HttpDelete("delete-photo/{photoId}")]  // api/users/delete-photo/11
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        // Get the user from the user repository and get the photo with the corresponding photo ID.
        var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("API ERROR: You cannot delete your main photo.");

        if (photo.PublicId != null)
        {
            var result = await this.photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null) return BadRequest("API ERROR: " + result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await this.userRepository.SaveAllAsync()) return Ok();

        return BadRequest("API ERROR: Problem deleting the photo.");
    }
}
