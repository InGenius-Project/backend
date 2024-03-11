namespace IngBackend.Profiles;

using AutoMapper;
using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;

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
        CreateMap<UserInfoPostDTO, User>();
        // .ForMember(dest => dest.Areas, opt => opt.Ignore())
        // .ForAllMembers(opts =>
        // {
        //     opts.AllowNull();
        //     opts.Condition((src, dest, srcMember) => srcMember != null);
        // });
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
        CreateMap<AreaDTO, Area>();
        CreateMap<ListLayout, ListLayout>();
        // .ForMember(dest => dest.Items, opt => opt.Ignore())
        // .ForMember(dest => dest.Area, opt => opt.Ignore())
        // .ForMember(dest => dest.AreaId, opt => opt.Ignore());
        CreateMap<Area, Area>();
        // .ForMember(dest => dest.ListLayout, opt => opt.Ignore())
        //  .ForAllMembers(opts =>
        // {
        //     opts.AllowNull();
        //     opts.Condition((src, dest, srcMember) => srcMember != null);
        // });
        CreateMap<AreaPostDTO, Area>();

        // .ForAllMembers(opts =>
        // {
        //     opts.AllowNull();
        //     opts.Condition((src, dest, srcMember) => srcMember != null);
        // });
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
        CreateMap<Tag, Tag>();
        // .ForMember(dest => dest.ListLayouts, opt => opt.Ignore())
        // .ForMember(dest => dest.Type, opt => opt.Ignore());
        CreateMap<TagType, TagTypeDTO>()
            .ReverseMap();
        CreateMap<TagTypeDTO, TagTypeDTO>();
        CreateMap<TagType, TagTypeDetailDTO>();
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
