using AutoMapper;
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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;
        private readonly ITagService _tagService;
        private readonly IStoryTagService _storyTagService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoryService(IStoryRepository storyRepository, IUnitOfWork unitOfWork, ITagService tagService, IStoryTagService storyTagService, IMapper mapper)
        {
            _storyRepository = storyRepository;
            _unitOfWork = unitOfWork;
            _tagService = tagService;
            _storyTagService = storyTagService;
            _mapper = mapper;
        }

        public async Task<int> CreateStoryAsync(CreateStoryRequest request, long userId)
        {
            try
            {

                var story = _mapper.Map<Story>(request);

                story.Slug = SlugGenerationHelper.GenerateSlug(request.Title);
                story.Summary = SummaryHelper.Generate(request.Content);
                story.UserId = userId;

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

        public async Task<PaginatedResponse<StoryResponse>> GetStoriesPreviewAsync(int limit, DateTime? cursor)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesPreviewAsync(limit, cursor);

                var hasMore = stories.Count > limit;
                var items = _mapper.Map<List<StoryResponse>>(stories.Take(limit).ToList());
                //var items = stories.Take(limit).Select(s => new StoryResponse
                //{
                //    Id = s.Id,
                //    Title = s.Title,
                //    Content = s.Content,
                //    CoverImageUrl = s.CoverImageUrl,
                //    CreatedAt = s.CreatedAt,
                //    LikeCount = s.LikeCount,
                //    CommentCount = s.CommentCount,
                //    User = new UserBasicResponse
                //    {
                //        Id = s.User.Id,
                //        Username = s.User.Username,
                //        AvatarUrl = s.User.AvatarUrl,
                //    },
                //    Media = _mapper.Map<List<MediaResponse>>(s.Media.ToList()),
                //    Tags = _mapper.Map<List<TagResponse>>(s.StoryTags.Select(st => st.Tag).ToList())
                //});

                var nextCursor = hasMore ? items.Last().CreatedAt.ToString("o") : null;

                return new PaginatedResponse<StoryResponse>
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

        public async Task<StoryResponse?> GetStoryByIdOrSlugAsync(int? storyId, string? slug)
        {
            try
            {
                var story = await _storyRepository.GetStoryByIdOrSlugAsync(storyId, slug);
                return story == null ? null : _mapper.Map<StoryResponse>(story);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> UpdateStoryAsync(CreateStoryRequest request, long userId, int storyId)
        {
            try
            {
                // Check if story exists
                var story = await _storyRepository.GetStoryByIdOrSlugAsync(storyId, null);
                if (story == null)
                    throw new Exception("Story not found");

                // Check user owns the story
                var isOwner = story.UserId == userId;
                if (!isOwner)
                    throw new Exception("You do not have permission to update this story");
               
                story.Title = request.Title;
                story.Slug = SlugGenerationHelper.GenerateSlug(request.Title);
                story.Content = request.Content;
                story.Summary = SummaryHelper.Generate(request.Content);
                story.CoverImageUrl = request.CoverImageUrl;
                story.PrivacyStatus = request.PrivacyStatus;
                story.StoryStatus = request.StoryStatus;
                story.LastUpdatedAt = DateTime.UtcNow;

                // Handle Tags
                // Get existing tags
                var existingTags = story.StoryTags.Select(st => st.Tag.Name).ToList();
                var newTags = request.Tags.Distinct().ToList();
                var tagsToAdd = newTags.Except(existingTags, StringComparer.OrdinalIgnoreCase).ToList();
                var tagsToRemove = existingTags.Except(newTags, StringComparer.OrdinalIgnoreCase).ToList();

                // Add new tags
                foreach (var tagName in tagsToAdd)
                {
                    var slug = SlugGenerationHelper.GenerateSlug(tagName);
                    var tag = await _tagService.GetTagAsync(tagName);

                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            Slug = slug,
                            IsUserGenerated = true
                        };
                        await _tagService.CreateTagAsync(tag);
                        await _unitOfWork.SaveAsync();
                    }

                    var storyTag = new StoryTag
                    {
                        StoryId = story.Id,
                        TagId = tag.Id
                    };

                    var exists = story.StoryTags
                                .Any(st => st.StoryId == storyTag.StoryId && st.TagId == storyTag.TagId);
                    if (!exists)
                    {
                        await _storyTagService.AddStoryTagAsync(storyTag);
                    }                                    
                }

                // Remove old tags
                foreach (var tagName in tagsToRemove)
                {
                    var tagId = await _tagService.GetTagIdByNameAsync(tagName);
                    await _storyTagService.RemoveStoryTagAsync(story.Id, tagId);
                }

                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
