namespace IngBackendApi.Profiles;

using AutoMapper;
using AutoMapper.EquivalencyExpression;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.DTO.HttpResponse;
using Microsoft.CodeAnalysis;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region User Mapping
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
        #endregion

        #region Resume Mapping
        CreateMap<Resume, ResumeDTO>().EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<ResumeDTO, Resume>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id)
            .ForMember(dest => dest.Recruitments, opt => opt.Ignore());
        CreateMap<ResumePostDTO, ResumeDTO>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<ResumePostDTO, Resume>()
            .ForMember(rp => rp.User, r => r.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id)
            .ForMember(dest => dest.Recruitments, opt => opt.Ignore());
        #endregion

        #region Recruitment Mapping
        CreateMap<Recruitment, RecruitmentDTO>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<RecruitmentDTO, Recruitment>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<RecruitmentPostDTO, RecruitmentDTO>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<RecruitmentPostDTO, Recruitment>()
            .ForMember(rp => rp.Publisher, r => r.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        #endregion

        #region Area Mapping
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
            .ForMember(dest => dest.ImageTextLayout, opt => opt.Ignore())
            .ForMember(dest => dest.ImageTextLayoutId, opt => opt.Ignore())
            .ForMember(dest => dest.TextLayoutId, opt => opt.Ignore())
            .ForMember(dest => dest.ListLayoutId, opt => opt.Ignore())
            .ForMember(dest => dest.KeyValueListLayoutId, opt => opt.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<AreaPostDTO, AreaDTO>().ForMember(dest => dest.ListLayout, opt => opt.Ignore());
        CreateMap<AreaDTO, Area>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImageTextLayout, opt => opt.Ignore())
            .ForMember(dest => dest.ImageTextLayoutId, opt => opt.Ignore())
            .ForMember(dest => dest.TextLayoutId, opt => opt.Ignore())
            .ForMember(dest => dest.ListLayoutId, opt => opt.Ignore())
            .ForMember(dest => dest.KeyValueListLayoutId, opt => opt.Ignore())
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<AreaType, AreaTypeDTO>();
        CreateMap<AreaTypeDTO, AreaType>().EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<AreaTypePostDTO, AreaType>()
            .EqualityComparison((dto, entity) => dto.Id == entity.Id);
        CreateMap<AreaTypePostDTO, AreaTypeDTO>();
        #endregion

        #region TextLayout Mapping
        CreateMap<TextLayoutDTO, TextLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<TextLayoutPostDTO, TextLayoutDTO>().ReverseMap();
        #endregion

        #region ImageLayout Mapping
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>().ReverseMap();
        CreateMap<ImageDTO, Image>();
        #endregion

        #region ListLayout Mapping
        CreateMap<ListLayoutDTO, ListLayout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<ListLayoutPostDTO, ListLayoutDTO>();
        #endregion

        #region KeyValueListLayout Mapping
        CreateMap<KeyValueListLayoutPostDTO, KeyValueListLayoutDTO>().ReverseMap();
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
        #endregion

        #region Tag Mapping
        CreateMap<TagDTO, Tag>().EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id));
        CreateMap<Tag, TagDTO>();
        CreateMap<TagPostDTO, TagDTO>().ReverseMap();
        CreateMap<TagPostDTO, Tag>().EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id));
        CreateMap<TagType, TagTypeDTO>()
            .EqualityComparison((dto, entity) => dto.Id.Equals(entity.Id))
            .ReverseMap();
        CreateMap<TagTypePostDTO, TagTypeDTO>().ReverseMap();
        #endregion

        #region User Info Area Mapping
        CreateMap<Area, UserInfoAreaDTO>()
            .ForMember(
                dest => dest.Title,
                opt => opt.MapFrom(src => src.AreaType != null ? src.AreaType.Name : "")
            )
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => GetLayoutContent(src)));
        #endregion

        #region AI Generated Area Mapping
        CreateMap<AiGeneratedAreaDTO, AreaDTO>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.AreaTitle))
            .ForMember(
                dest => dest.TextLayout,
                opt => opt.MapFrom(src => new TextLayoutDTO { Content = src.Content })
            );
        #endregion

        #region AI Analyze
        CreateMap<SafetyReportResponse, SafetyReport>();
        #endregion

        #region Chat
        CreateMap<ChatGroup, ChatGroupInfoDTO>()
            .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.Private));
        CreateMap<ChatGroup, ChatGroupDTO>();
        CreateMap<ChatMessage, ChatMessageDTO>()
            .ForMember(dest => dest.SendTime, opt => opt.MapFrom(src => src.CreatedAt));
        #endregion
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

    private static string? GetImageUri(IConfiguration configuration, Image image)
    {
        if (image.Uri != null)
        {
            return image.Uri;
        }
        else if (image.Filepath.Contains("images/avatars"))
        {
            return $"{configuration["Domain:Url"]}api/user/avatar?imageId={image.Id}";
        }
        return $"{configuration["Domain:Url"]}api/area/image?id={image.Id}";
    }

    private static string GetLayoutContent(Area area)
    {
        if (area.TextLayout != null)
        {
            return area.TextLayout.Content;
        }

        if (area.ListLayout != null)
        {
            if (area.ListLayout.Items == null || area.ListLayout.Items.Count == 0)
            {
                return "";
            }
            return string.Join(", ", area.ListLayout.Items.Select(i => i.Name));
        }

        if (area.ImageTextLayout != null)
        {
            return area.ImageTextLayout.TextContent ?? "";
        }

        if (area.KeyValueListLayout != null)
        {
            if (area.KeyValueListLayout.Items == null || area.KeyValueListLayout.Items.Count == 0)
            {
                return "";
            }
            return string.Join(
                ", ",
                area.KeyValueListLayout.Items.SelectMany(i => i.Key.Select(k => k.Name))
                    .Where(i => i != null)
            );
        }
        return "";
    }
}
