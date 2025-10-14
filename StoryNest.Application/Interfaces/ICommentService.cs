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
    public interface ICommentService
    {
        public Task<CommentResponse> AddCommentAsync(CreateCommentRequest request, int storyId, long userId);
        public Task<bool> UpdateCommentAsync(CreateCommentRequest request, int commentId, long userId);
        public Task<bool> DeleteCommentAsync(int commentId, long userId);
        Task<PaginatedResponse<CommentResponse>> GetCommentsAsync(int storyId, int? parentId, int limit, int? cursor);
    }
}
