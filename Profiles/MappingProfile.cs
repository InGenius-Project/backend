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
        CreateMap<AreaFormDataDTO, Area>()
            .ForMember(
                des => des.ResumeId,
                opt => opt.MapFrom(s => s.AreaPost.ResumeId)
            )
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
                opt => {
                    opt.Condition((s, d) => d.ImageTextLayout != null && s.AreaPost.ImageTextLayout != null && s.Image != null);
                    opt.MapFrom(s => CombineImageTextLayout(s.AreaPost.ImageTextLayout, s.Image));
                }
            );

        CreateMap<ImageDTO, Image>()
            .ForMember(
                dest => dest.Filename,
                opt => opt.MapFrom(src => src.File.FileName)
            ).ForMember(
                dest => dest.ContentType,
                opt => opt.MapFrom(src => src.File.ContentType)
            ).ForMember(
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
