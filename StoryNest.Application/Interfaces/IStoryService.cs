using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IStoryService
    {
        public Task<int> CreateStoryAsync(CreateStoryRequest request);
        public Task<PaginatedResponse<StoryPreviewResponse>> GetStoriesPreviewAsync(int limit, DateTime? cursor);
    }
}
