using AutoMapper;
using AutoMapper.EquivalencyExpression;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

namespace IngBackendApi.Profiles;

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
                opt => opt.MapFrom(src => src.AreaType == null ? src.LayoutType : src.AreaType.LayoutType)
            );
        CreateMap<AreaPostDTO, Area>();
        CreateMap<AreaPostDTO, AreaDTO>()
            .ForMember(dest => dest.ListLayout, opt => opt.Ignore());

        CreateMap<AreaDTO, Area>();
        CreateMap<AreaType, AreaTypeDTO>();
        CreateMap<AreaTypeDTO, AreaType>().EqualityComparison((dto, entity) => dto.Id == entity.Id);

        CreateMap<AreaTypePostDTO, AreaType>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);

        CreateMap<AreaTypePostDTO, AreaTypeDTO>();

        // TextLayout
        CreateMap<TextLayoutDTO, TextLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();


        CreateMap<TextLayoutPostDTO, TextLayoutDTO>()
            .ReverseMap();

        // ImageLayout
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>()
            .ReverseMap();
        CreateMap<ImageDTO, Image>().ReverseMap();

        // ListLayout
        CreateMap<ListLayoutDTO, ListLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
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
        CreateMap<TagType, TagTypeDTO>()
            .EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id))
            .ReverseMap();

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
