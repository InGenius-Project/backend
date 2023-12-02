using AutoMapper;
using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;

namespace IngBackend.Profiles;

public class MappingProfile : Profile

{
    public MappingProfile()
    {
        CreateMap<UserInfoDTO, UserDTO>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
        CreateMap<TokenDTO, UserDTO>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src));


        // Resume
        CreateMap<ResumeDTO, Resume>();
        CreateMap<Resume, ResumeDTO>();
        CreateMap<ResumePostDTO, Resume>();
        CreateMap<TextLayoutDTO, TextLayout>();
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>();
        CreateMap<ImageDTO, Image>();
        CreateMap(typeof(ResumeAreaDTO<>), typeof(ResumeAreaDTO<>));


    }
    public MappingProfile(IPasswordHasher passwordHasher) : this()
    {
        CreateMap<UserSignUpDTO, User>()
           .ForMember(
                dest => dest.HashedPassword,
                opt => opt.MapFrom(src => passwordHasher.HashPassword(src.Password)));
        CreateMap<User, UserInfoDTO>();
    }
}
