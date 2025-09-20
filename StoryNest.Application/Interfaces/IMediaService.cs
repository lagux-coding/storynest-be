using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IMediaService
    {
        Task<int> CreateMediaAsync(int storyId, List<string> url);
        Task<int> DeleteMediaByStoryIdAsync(int storyId);
    }
}
