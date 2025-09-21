using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface ICommentRepository
    {
        public Task<Comment?> GetByIdAsync(int commentId);
        public Task<List<Comment>> GetByStoryId(int storyId, int? parentId, int limit, int offset);
        public Task AddAsync(Comment comment);
        public Task UpdateAsync(Comment comment);
        public Task<List<int>> GetCommentIdsWithReplies(List<int> ids);
        public Task<Dictionary<int, int>> GetRepliesCount(List<int> parentId);
    }
}
