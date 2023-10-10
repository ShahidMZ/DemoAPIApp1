using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        // Map properties of the first class to the properties of the second class using the CreateMap() method.
        // Also, map the main photo's URL to the PhotoUrl property of the user using the ForMember() method and specifying the destination member and destination options.
        // Use the DateOfBirth.CalculateAge() method to map the age to the MemberDTO.
        CreateMap<AppUser, MemberDTO>()
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
        CreateMap<Photo, PhotoDTO>();
        // Map the content of MemberUpdate DTO (which has new data from the client) to the corresponding fields in the AppUser.
        CreateMap<MemberUpdateDTO, AppUser>();
    }
}
