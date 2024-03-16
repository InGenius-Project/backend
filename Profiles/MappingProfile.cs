using AutoMapper;
using AutoMapper.EquivalencyExpression;
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
            .ForMember(
                dest => dest.Title,
                opt => opt.MapFrom(src => src.AreaType == null ? src.Title : src.AreaType.Name)
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
            .EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id));
        CreateMap<AreaTypePostDTO, AreaType>()
            .EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id));

        CreateMap<AreaTypePostDTO, AreaTypeDTO>();

        // TextLayout
        CreateMap<TextLayoutDTO, TextLayout>()
            .ReverseMap();

        // ImageLayout
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>()
            .ReverseMap();
        CreateMap<ImageDTO, Image>().ReverseMap();

        // ListLayout
        CreateMap<ListLayoutDTO, ListLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AreaId, opt => opt.Ignore())
            .ForMember(dest => dest.Area, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<ListLayoutPostDTO, ListLayoutDTO>();

        // KeyValueListLayout
        CreateMap<KeyValueListLayoutDTO, KeyValueListLayout>()
            .ReverseMap();
        CreateMap<KeyValueItem, KeyValueItemDTO>().ReverseMap();

        // Tag
        CreateMap<TagDTO, Tag>()
            .EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id));

        CreateMap<Tag, TagDTO>();

        CreateMap<TagPostDTO, TagDTO>().ReverseMap();
        CreateMap<TagType, TagTypeDTO>().ReverseMap();
        CreateMap<TagTypePostDTO, TagTypeDTO>().ReverseMap();
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
