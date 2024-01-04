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
        CreateMap<Resume, ResumeDTO>();
        CreateMap<ResumePostDTO, Resume>();

        // Area
        CreateMap<Area, AreaDTO>();
        CreateMap<AreaDTO, Area>();
        CreateMap<AreaPostDTO, Area>();
        // TextLayout
        CreateMap<TextLayoutDTO, TextLayout>();
        CreateMap<TextLayout, TextLayoutDTO>();
        // ImageLayout
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>();
        CreateMap<ImageTextLayout, ImageTextLayoutDTO>();
        // ListLayout
        CreateMap<ListLayoutDTO, ListLayout>();
        CreateMap<ListLayout, ListLayoutDTO>();
        // KeyValueListLayout
        CreateMap<KeyValueListLayoutDTO, KeyValueListLayout>();
        CreateMap<KeyValueListLayout, KeyValueListLayoutDTO>();
        CreateMap<KeyValueItem, KeyValueItemDTO>();
        CreateMap<KeyValueItemDTO, KeyValueItem>();

        // Tag
        CreateMap<Tag, TagDTO>();
        CreateMap<TagDTO, Tag>();

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
