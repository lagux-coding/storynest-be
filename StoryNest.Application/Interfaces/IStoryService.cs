using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
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
        public Task<int> UpdateWithEntityAsync(Story story);
        public Task<int> RemoveStoryAsync(int storyId, long userId);
        public Task<PaginatedResponse<StoryResponse>> GetStoriesPreviewAsync(int limit, DateTime? cursor, long? userId = null);
        public Task<StoryResponse?> GetStoryByIdOrSlugAsync(int? storyId, string? slug);
        public Task<Story> GetStoryByIdAsync(int storyId);
        public Task<PaginatedResponse<StoryResponse>> GetStoriesByUserAsync(long userId, DateTime? cursor, long? userLikeId = null);
        public Task<PaginatedResponse<StoryResponse>> GetStoriesByOwnerAsync(long userId, DateTime? cursor, long userLikeId);
        public Task<PaginatedResponse<StoryResponse>> GetStoriesByUserAIAsync(long userId, DateTime? cursor, long? userLikeId = null);
        public Task<PaginatedResponse<StoryResponse>> GetStoriesByOwnerAIAsync(long userId, DateTime? cursor, long userLikeId);
        public Task<StorySearchResult> SearchStoriesAsync(string keyword, int limit = 20, int? lastId = null);
    }
}
