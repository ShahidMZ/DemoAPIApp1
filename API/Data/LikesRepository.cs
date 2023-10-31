using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    // Implemented in Chapter 176: Implementing the likes repository.

    private readonly DataContext context;

    public LikesRepository(DataContext context)
    {
        this.context = context;
    }

    // Find the UserLike entity that matches the primary key which is a combination of the sourceUserId and targetUserId parameters.
    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await this.context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    // Get the list of users liked by the currently logged in user or the list of users who have liked the currently logged in user.
    // The userId parameter is the ID of the currently logged in user.
    // The predicate parameter is either "liked" or "likedBy".
    // The userId could be the sourceUserId, which means get the list of users liked by the currently logged in user.
    // Or, it could be the targetUserId, which means get the list of users who have liked the currently logged in user.
    // This is specified by the predicate parameter.
    public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
    {
        // Create queries for the users and likes.
        // AsQueryable() means the query has not been executed yet. The DB has not been queried at this point.
        IQueryable<AppUser> users = this.context.Users.OrderBy(u => u.UserName).AsQueryable();
        IQueryable<UserLike> likes = this.context.Likes.AsQueryable();

        // If the list of users liked by the current user is to be returned.
        if (likesParams.Predicate == "liked")
        {
            // Get the UserLikes from the Likes table where the sourceId is the current user's ID.
            likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
            // Get the AppUsers from the UserLikes in the likes variable.
            users = likes.Select(like => like.TargetUser);
        }

        // If the list of users who like the current user is to be returned.
        if (likesParams.Predicate == "likedBy")
        {
            // Get the UserLikes from the LIkes table where the targetId is the current User's ID.
            likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
            // Get the AppUsers from the UserLikes in the likes variable.
            users = likes.Select(like => like.SourceUser);
        }

        // For every user in the users variable, create a LikeDTO and return the list.
        var likedUsers = users.Select(user => new LikeDTO
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
            City = user.City,
            Id = user.Id
        });

        return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    // Return the user specified by the userId with the LikedUsers eagerly loaded.
    public async Task<AppUser> GetUserWithLikes(int userId)
    {
        return await this.context.Users
            // Eager loading the LikedUsers
            .Include(x => x.LikedUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
