using StoryNest.Application.Dtos.Request;
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
    }
}
