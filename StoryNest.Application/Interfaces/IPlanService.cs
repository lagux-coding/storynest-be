using StoryNest.Application.Dtos.Request;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IPlanService
    {
        public Task<int> AddPlanAsync(CreatePlanRequest request);
        public Task<Plan> GetPlanByIdAsync(int planId);
    }
}
