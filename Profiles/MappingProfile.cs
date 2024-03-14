using AutoMapper;
using IngBackend.Interfaces.Service;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;

namespace IngBackend.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserInfoDTO, UserDTO>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
        CreateMap<User, UserInfoDTO>();
        CreateMap<UserInfoDTO, User>()
            .ForAllMembers(opts =>
            {
                opts.AllowNull();
                opts.Condition((src, dest, srcMember) => srcMember != null);
            });
        CreateMap<UserInfoPostDTO, User>()
            // .ForMember(dest => dest.Areas, opt => opt.Ignore())
            .ForAllMembers(opts =>
            {
                opts.AllowNull();
                opts.Condition((src, dest, srcMember) => srcMember != null);
            });
        CreateMap<UserSignUpDTO, UserInfoDTO>();

        CreateMap<TokenDTO, UserDTO>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src));

        // Resume
        CreateMap<Resume, ResumeDTO>();
        CreateMap<ResumePostDTO, Resume>();

        // Recruitment
        CreateMap<Recruitment, RecruitmentDTO>()
            .ReverseMap();
        CreateMap<RecruitmentPostDTO, Recruitment>().ForMember(rp => rp.Publisher, r => r.Ignore());

        // Area
        CreateMap<Area, AreaDTO>()
            .ForMember(dest => dest.AreaTypeId, opt => opt.MapFrom(src => src.AreaType.Id))
            .ForMember(
                dest => dest.Title,
                opt =>
                {
                    opt.Condition((src, dest, srcMember) => src.AreaType != null);
                    opt.MapFrom(src => src.AreaType!.Name);
                }
            )
            .ForMember(
                dest => dest.LayoutType,
                opt =>
                {
                    opt.Condition((src, dest, srcMember) => src.AreaType != null);
                    opt.MapFrom(src => src.AreaType!.LayoutType);
                }
            );
        CreateMap<AreaPostDTO, Area>();
        CreateMap<AreaPostDTO, AreaDTO>();

        CreateMap<AreaDTO, Area>();
        CreateMap<AreaType, AreaTypeDTO>();
        CreateMap<AreaTypeDTO, AreaType>()
            .ForMember(dest => dest.ListTagTypes, opt => opt.Ignore());
        CreateMap<AreaTypePostDTO, AreaType>()
            .ForMember(dest => dest.ListTagTypes, opt => opt.Ignore());

        // TextLayout
        CreateMap<TextLayoutDTO, TextLayout>()
            .ReverseMap();

        // ImageLayout
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>()
            .ReverseMap();
        CreateMap<ImageDTO, Image>().ReverseMap();

        // ListLayout
        CreateMap<ListLayoutDTO, ListLayout>()
            .ReverseMap();

        // KeyValueListLayout
        CreateMap<KeyValueListLayoutDTO, KeyValueListLayout>()
            .ReverseMap();
        CreateMap<KeyValueItem, KeyValueItemDTO>().ReverseMap();

        // Tag
        CreateMap<Tag, TagDTO>()
            .ReverseMap();
        CreateMap<TagType, TagTypeDTO>().ReverseMap();
    }

    public MappingProfile(IPasswordHasher passwordHasher)
        : this()
    {
        // _unitOfWork = unitOfWork;
        CreateMap<UserSignUpDTO, User>()
            .ForMember(
                dest => dest.HashedPassword,
                opt => opt.MapFrom(src => passwordHasher.HashPassword(src.Password))
            );
        CreateMap<User, UserInfoDTO>();
    }
}
