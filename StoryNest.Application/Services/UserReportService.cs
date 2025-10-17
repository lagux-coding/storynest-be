using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStoryService _storyService;

        public UserReportService(IUserReportRepository userReportRepository, IUnitOfWork unitOfWork, IStoryService storyService)
        {
            _userReportRepository = userReportRepository;
            _unitOfWork = unitOfWork;
            _storyService = storyService;
        }

        public async Task<int> CreateReportAsync(UserReportRequest request, long reporterId, int storyId, int commentId = 0)
        {
            try
            {
                var story = await _storyService.GetStoryByIdAsync(storyId);

                UserReport report = new()
                {
                    ReportedId = story.UserId,
                    ReporterId = reporterId,
                    ReportedStoryId = storyId,
                    Reason = request.Reason,
                    Status = ReportStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };

                if (commentId > 0)
                {
                    report.ReportedCommentId = commentId;
                }

                await _userReportRepository.CreateUserReportAsync(report);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }
    }
}
