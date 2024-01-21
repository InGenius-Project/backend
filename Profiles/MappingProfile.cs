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
        CreateMap<UserInfoPostDTO, User>();
        CreateMap<TokenDTO, UserDTO>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src));

        // Resume
        CreateMap<Resume, ResumeDTO>();
        CreateMap<ResumePostDTO, Resume>();

        // Recruitment
        CreateMap<Recruitment, RecruitmentDTO>()
            .ReverseMap();
        CreateMap<RecruitmentPostDTO, Recruitment>()
            .ForMember(rp => rp.Publisher, r => r.Ignore());

        // Area
        CreateMap<Area, AreaDTO>()
            .ReverseMap();
        CreateMap<AreaPostDTO, Area>();

        // TextLayout
        CreateMap<TextLayoutDTO, TextLayout>()
            .ReverseMap();

        // ImageLayout
        CreateMap<ImageTextLayoutDTO, ImageTextLayout>()
            .ReverseMap();

        // ListLayout
        CreateMap<ListLayoutDTO, ListLayout>()
            .ReverseMap();

        // KeyValueListLayout
        CreateMap<KeyValueListLayoutDTO, KeyValueListLayout>()
            .ReverseMap();
        CreateMap<KeyValueItem, KeyValueItemDTO>()
            .ReverseMap();

        // Tag
        CreateMap<Tag, TagDTO>()
            .ReverseMap();

        CreateMap<AreaFormDataDTO, Area>()
            .ForMember(
                des => des.TextLayout,
                opt => opt.MapFrom(s => s.AreaPost.TextLayout)
            )
            .ForMember(
                des => des.Sequence,
                opt => opt.MapFrom(s => s.AreaPost.Sequence)
            )
            .ForMember(
                des => des.IsDisplayed,
                opt => opt.MapFrom(s => s.AreaPost.IsDisplayed)
            )
            .ForMember(
                des => des.ImageTextLayout,
                opt =>
                {
                    opt.Condition((s, d) => d.ImageTextLayout != null && s.AreaPost.ImageTextLayout != null && s.Image != null);
                    opt.MapFrom(s => CombineImageTextLayout(s.AreaPost.ImageTextLayout, s.Image));
                }
            );

        CreateMap<ImageDTO, Image>()
            .ForMember(
                dest => dest.Filename,
                opt => opt.MapFrom(src => src.File.FileName)
            )
            .ForMember(
                dest => dest.ContentType,
                opt => opt.MapFrom(src => src.File.ContentType)
            )
            .ForMember(
                dest => dest.Content,
                opt => opt.MapFrom(src => GetImageArray(src.File))
            );
    }
    public MappingProfile(IPasswordHasher passwordHasher) : this()
    {
        CreateMap<UserSignUpDTO, User>()
           .ForMember(
                dest => dest.HashedPassword,
                opt => opt.MapFrom(src => passwordHasher.HashPassword(src.Password)));
        CreateMap<User, UserInfoDTO>();
    }

    private static byte[] GetImageArray(IFormFile Image)
    {
        MemoryStream ms = new();
        Image.CopyTo(ms);
        return ms.ToArray();
    }

    private ImageTextLayoutDTO CombineImageTextLayout(ImageTextLayoutDTO itl, IFormFile image)
    {
        itl.Image = new ImageDTO() { File = image };
        return itl;
    }
}
