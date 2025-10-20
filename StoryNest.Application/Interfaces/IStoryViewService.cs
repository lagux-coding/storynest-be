using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IStoryViewService
    {
        public Task LogStoryViewAsync(int storyId, long userId, string? ip = null, string? device = null);
    }
}
