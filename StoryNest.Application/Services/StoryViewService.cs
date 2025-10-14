using StoryNest.Application.Interfaces;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class StoryViewService : IStoryViewService
    {
        private readonly IStoryViewRepository _storyViewRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StoryViewService(IStoryViewRepository storyViewRepository, IUnitOfWork unitOfWork)
        {
            _storyViewRepository = storyViewRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task LogStoryViewAsync(int storyId, long userId, string? ip = null, string? device = null)
        {
            try
            {
                await _storyViewRepository.AddStoryViewLogAsync(storyId, userId, ip, device);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
