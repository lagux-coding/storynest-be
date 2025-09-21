using AutoMapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Mappings
{
    public class StoryProfile : Profile
    {
        public StoryProfile()
        {
            CreateMap<CreateStoryRequest, Story>();
            CreateMap<Story, StoryResponse>()
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CoverImageUrl) ? null : $"https://cdn.storynest.io.vn/{src.CoverImageUrl}"))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.Media))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.StoryTags.Select(st => st.Tag)));
            CreateMap<Media, MediaResponse>()
                .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.MediaUrl) ? null : $"https://cdn.storynest.io.vn/{src.MediaUrl}"));
            CreateMap<Tag, TagResponse>();
            CreateMap<Comment, CommentResponse>();
        }
    }
}
