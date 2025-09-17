using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;
        private readonly ITagService _tagService;
        private readonly IStoryTagService _storyTagService;
        private readonly IUnitOfWork _unitOfWork;

        public StoryService(IStoryRepository storyRepository, IUnitOfWork unitOfWork, ITagService tagService, IStoryTagService storyTagService)
        {
            _storyRepository = storyRepository;
            _unitOfWork = unitOfWork;
            _tagService = tagService;
            _storyTagService = storyTagService;
        }

        public async Task<int> CreateStoryAsync(CreateStoryRequest request, long userId)
        {
            try
            {
                var story = new Story
                {
                    Title = request.Title,
                    Slug = SlugGenerationHelper.GenerateSlug(request.Title),
                    Content = request.Content,
                    Summary = SummaryHelper.Generate(request.Content),
                    CoverImageUrl = request.CoverImageUrl,
                    PrivacyStatus = request.PrivacyStatus,
                    StoryStatus = request.StoryStatus,
                    UserId = userId,
                };
                await _storyRepository.AddAsync(story);
                await _unitOfWork.SaveAsync();

                foreach (var tagName in request.Tags.Distinct())
                {
                    var slug = SlugGenerationHelper.GenerateSlug(tagName);

                    var tag = await _tagService.GetTagAsync(tagName);

                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            Slug = slug,
                            IsUserGenerated = true,
                        };

                        await _tagService.CreateTagAsync(tag);
                        await _unitOfWork.SaveAsync();
                    }

                    var storyTag = new StoryTag
                    {
                        StoryId = story.Id,
                        TagId = tag.Id,
                    };

                    await _storyTagService.AddStoryTagAsync(storyTag);
                }

                return await _unitOfWork.SaveAsync();
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginatedResponse<StoryPreviewResponse>> GetStoriesPreviewAsync(int limit, DateTime? cursor)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesPreviewAsync(limit, cursor);

                var hasMore = stories.Count > limit;
                var items = stories.Take(limit).Select(s => new StoryPreviewResponse
                {
                    Id = s.Id,
                    Title = s.Title,
                    Summary = s.Summary,
                    CoverImageUrl = s.CoverImageUrl,
                    CreatedAt = s.CreatedAt,
                });

                var nextCursor = hasMore ? items.Last().CreatedAt.ToString("o") : null;

                return new PaginatedResponse<StoryPreviewResponse>
                {
                    Items = items,
                    NextCursor = nextCursor,
                    HasMore = hasMore,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
