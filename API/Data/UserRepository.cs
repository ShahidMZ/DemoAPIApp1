using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

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
        // Project the AppUser to the MemberDTO. Eager Loading of the photos is ont required when you're projecting.
        return await this.context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
    {
        return await this.context.Users
            .ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        // Include(p => p.Photos) notifies the Entity Framework to get the related data fields. This is called Eager Loading.
        // return await this.context.Users.FindAsync(id);
        return await this.context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.Id == id);
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
