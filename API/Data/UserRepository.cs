using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext context;
    private readonly IMapper mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public async Task<MemberDTO> GetMemberAsync(string username)
    {
        // Project the AppUser to the MemberDTO. Eager Loading of the photos is not required when you're projecting.
        return await this.context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    // GetMembersAsync() method before pagination
    // public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
    // {
    //     return await this.context.Users
    //         .ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider).ToListAsync();
    // }

    public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
    {
        var query = this.context.Users.AsQueryable();

        // Exclude the current user from the results to be returned.
        query = query.Where(u => u.UserName != userParams.CurrentUserName);

        // Include only the users whose gender matches the one specified in the userParams.
        // If the userParams has the gender as all, skip this step to include all genders.
        if (!userParams.Gender.Equals("all"))
        {
            query = query.Where(u => u.Gender == userParams.Gender);
        }

        // Get the age ranges
        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge - 1));
        
        // Include only users whose DOB falls in the age range.
        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth < maxDob);

        // Sort
        query = userParams.OrderBy switch
        {
            // Sort by Created
            "created" => query.OrderByDescending(u => u.Created),
            // Default: Sort by LastActive
            _ => query.OrderByDescending(u => u.LastActive)
        };

        // The above switch code works in the same way as below.
        // switch (userParams.OrderBy)
        // {
        //     case "created": 
        //         query = query.OrderByDescending(u => u.Created);
        //         break;
        //     default:
        //         query = query.OrderByDescending(u => u.LastActive);
        //         break;
        // }

        return await PagedList<MemberDTO>.CreateAsync(
            query.ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider).AsNoTracking(), 
            userParams.PageNumber, userParams.PageSize);

        // Before Filtering was added (Chapter 160: Adding filtering to the API):
        // Build a query with all the users and project from AppUser to MemberDTO.
        // var query = this.context.Users
        //     .ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider)
        //     .AsNoTracking();
        // Since the list being returned won't be used by the users controller, specifying AsNoTracking() will stop the EF from tracking it and make it a tiny bit more efficient.
        // This is optional.
        
        // return await PagedList<MemberDTO>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        // Include(p => p.Photos) notifies the Entity Framework to get the related data fields. This is called Eager Loading.
        return await this.context.Users.FindAsync(id);
        // return await this.context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        // Include(p => p.Photos) notifies the Entity Framework to get the related data fields. This is called Eager Loading.
        return await this.context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        // Include(p => p.Photos) notifies the Entity Framework to get the related data fields.
        return await this.context.Users.Include(p => p.Photos).ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        // The SaveChangesAsync() method returns the number of entries written to the DB. If something has been written, return true.
        return await this.context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
        // Inform the Entity Framework tracker that an entity has been updated.
        context.Entry(user).State = EntityState.Modified;
    }
}
