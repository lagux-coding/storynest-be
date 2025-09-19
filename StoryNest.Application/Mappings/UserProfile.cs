using AngleSharp;
using AutoMapper;
using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserBasicResponse, User>();
            CreateMap<User, UserBasicResponse>()
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.AvatarUrl) ? null : $"https://cdn.storynest.io.vn/{src.AvatarUrl}"));
        }
    }
}
