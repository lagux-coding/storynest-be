using AutoMapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Mappings
{
    public class PlanProfile : Profile
    {
        public PlanProfile()
        {
            CreateMap<CreatePlanRequest, Plan>();
        }
    }
}
