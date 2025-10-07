using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class StoryTagService : IStoryTagService
    {
        private readonly IStoryTagRepository _storyTagRepository;

        public StoryTagService(IStoryTagRepository storyTagRepository)
        {
            _storyTagRepository = storyTagRepository;
        }

        public async Task AddStoryTagAsync(StoryTag storyTag)
        {
        
            try
            {
                await _storyTagRepository.AddAsync(storyTag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<bool> GetStoryTagAsync(int storyId = 0, int tagId = 0)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveStoryTagAsync(int storyId, int tagId)
        {
            await _storyTagRepository.RemoveAsync(storyId, tagId);
        }
    }
}
