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
        public Task<int> CreateStoryAsync(CreateStoryRequest request, long userId);
        public Task<int> UpdateStoryAsync(CreateStoryRequest request, long userId, int storyId);
        public Task<PaginatedResponse<StoryResponse>> GetStoriesPreviewAsync(int limit, DateTime? cursor);
        public Task<StoryResponse?> GetStoryByIdOrSlugAsync(int? storyId, string? slug);
    }
}
