namespace IngBackendApi.Profiles;

using AutoMapper;
using AutoMapper.EquivalencyExpression;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserInfoDTO, UserDTO>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
        CreateMap<User, UserInfoDTO>().EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<User, OwnerUserDTO>().EqualityComparison((dto, entity) => dto.Id == entity.Id);
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
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<RecruitmentDTO, Recruitment>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<RecruitmentPostDTO, RecruitmentDTO>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<RecruitmentPostDTO, Recruitment>()
            .ForMember(rp => rp.Publisher, r => r.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);

        // Area
        CreateMap<Area, AreaDTO>()
            .ForMember(
                dest => dest.Title,
                opt => opt.MapFrom(src => src.AreaType == null ? src.Title : src.AreaType.Name)
            )
            .ForMember(
                dest => dest.LayoutType,
                opt =>
                    opt.MapFrom(src =>
                        src.AreaType == null ? src.LayoutType : src.AreaType.LayoutType
                    )
            );
        CreateMap<AreaPostDTO, Area>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ListLayoutId, opt => opt.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);

        CreateMap<AreaPostDTO, AreaDTO>().ForMember(dest => dest.ListLayout, opt => opt.Ignore());

        CreateMap<AreaDTO, Area>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImageTextLayout, opt => opt.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<AreaType, AreaTypeDTO>();
        CreateMap<AreaTypeDTO, AreaType>().EqualityComparison((dto, entity) => dto.Id == entity.Id);

        CreateMap<AreaTypePostDTO, AreaType>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);

        CreateMap<AreaTypePostDTO, AreaTypeDTO>();

        // TextLayout
        CreateMap<TextLayoutDTO, TextLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<TextLayoutPostDTO, TextLayoutDTO>().ReverseMap();

        // ImageLayout
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>()
            .ReverseMap();

        CreateMap<ImageDTO, Image>();

        // ListLayout
        CreateMap<ListLayoutDTO, ListLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<ListLayoutPostDTO, ListLayoutDTO>();

        // KeyValueListLayout
        CreateMap<KeyValueListLayoutPostDTO, KeyValueListLayoutDTO>()
            .ReverseMap();

        CreateMap<KeyValueListLayoutDTO, KeyValueListLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id)
            .ReverseMap();
        CreateMap<KeyValueItemPostDTO, KeyValueItemDTO>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<KeyValueItemDTO, KeyValueItem>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<KeyValueItem, KeyValueItemDTO>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);

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

    public MappingProfile(IConfiguration configuration)
        : this()
    {
        CreateMap<Image, ImageDTO>()
            .ForMember(
                dest => dest.Uri,
                opt => opt.MapFrom(src => GetImageUri(configuration, src))
            );

        CreateMap<Image, ImageInfo>()
            .ForMember(
                dest => dest.Uri,
                opt => opt.MapFrom(src => GetImageUri(configuration, src))
            );
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
        CreateMap<User, UserInfoDTO>().EqualityComparison((dto, entity) => dto.Id == entity.Id);
    }

    private static string GetImageUri(IConfiguration configuration, Image image)
    {
        if (image.Filepath.Contains("images/avatars"))
        {
            return string.Format(
                "{0}api/user/avatar?imageId={1}",
                configuration["Domain:Url"],
                image.Id
            );
        }
        return string.Format("{0}api/area/image?id={1}", configuration["Domain:Url"], image.Id);
    }
}
