using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IStoryViewRepository
    {
        public Task AddStoryViewLogAsync(int storyId, long userId, string? ip = null, string? device = null);
    }
}
