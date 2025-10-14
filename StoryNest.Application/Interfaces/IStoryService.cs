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
        public Task<PaginatedResponse<StoryResponse>> GetStoriesPreviewAsync(int limit, long cursor = 0, long? userId = null);
        public Task<PaginatedResponse<StoryResponse>> GetSmartStoriesAsync(int limit, long cursor = 0, long? userId = null);
        public Task<GetStoryResponse?> GetStoryByIdOrSlugAsync(int? storyId, string? slug);
        public Task<GetStoryResponse?> GetStoryByIdOrSlugAndStoryViewLogAsync(int? storyId, string? slug, long userId);
        public Task<GetStoryResponse?> GetStoryByIdOrSlugOwnerAsync(int? storyId, string? slug, long userId);
        public Task<Story> GetStoryByIdAsync(int storyId);
        public Task<PaginatedResponse<GetStoryResponse>> GetStoriesByUserAsync(long userId, DateTime? cursor, long? userLikeId = null);
        public Task<PaginatedResponse<GetStoryResponse>> GetStoriesByOwnerAsync(long userId, DateTime? cursor, long userLikeId);
        public Task<PaginatedResponse<GetStoryResponse>> GetStoriesByUserAIAsync(long userId, DateTime? cursor, long? userLikeId = null);
        public Task<PaginatedResponse<GetStoryResponse>> GetStoriesByOwnerAIAsync(long userId, DateTime? cursor, long userLikeId);
        public Task<StorySearchResult> SearchStoriesAsync(string keyword, int limit = 20, int? lastId = null);
    }
}
