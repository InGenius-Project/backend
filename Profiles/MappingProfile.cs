﻿using AutoMapper;
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

        CreateMap<AreaPostDTO, Area>();
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
