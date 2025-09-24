using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;

namespace StoryNest.Application.Services
{
    public class UserMediaService : IUserMediaService
    {
        private readonly IUserMediaRepository _userMediaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserMediaService(IUserMediaRepository userMediaRepository, IUnitOfWork unitOfWork)
        {
            _userMediaRepository = userMediaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddUserMedia(long userId, string key, MediaType type, UserMediaStatus status)
        {
            try
            {
                bool check = await _userMediaRepository.ExistsAsync(um => um.UserId == userId && um.MediaUrl == key && um.MediaType == type);
                if (check)
                    return 0;

                UserMedia media = new()
                {
                    UserId = userId,
                    MediaUrl = key,
                    MediaType = type,
                    CreatedAt = DateTime.UtcNow,
                    Status = status
                };
                await _userMediaRepository.AddAsync(media);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
