using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Data;

public class Repository : IUserRepository
{
    public Task<MemberDTO> GetMemberAsync(string username)
    {
        throw new NotImplementedException();
    }

    public Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
    {
        throw new NotImplementedException();
    }

    public Task<AppUser> GetUserByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<AppUser> GetUserByUsernameAsync(string username)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveAllAsync()
    {
        throw new NotImplementedException();
    }

    public void Update(API.Entities.AppUser user)
    {
        
    }
}
