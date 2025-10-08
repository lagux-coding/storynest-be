using AngleSharp.Io;
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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IUserMediaService _userMediaService;
        private readonly HttpClient _httpClient;
        private readonly IStoryRepository _storyRepository;
        private readonly ITagService _tagService;
        private readonly IStoryTagService _storyTagService;
        private readonly IMediaService _mediaService;
        private readonly IS3Service _s3Service;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoryService(IStoryRepository storyRepository, IUnitOfWork unitOfWork, ITagService tagService, IStoryTagService storyTagService, IMapper mapper, IMediaService mediaService, IUserMediaService userMediaService, HttpClient httpClient, IS3Service s3Service)
        {
            _storyRepository = storyRepository;
            _unitOfWork = unitOfWork;
            _tagService = tagService;
            _storyTagService = storyTagService;
            _mapper = mapper;
            _mediaService = mediaService;
            _userMediaService = userMediaService;
            _httpClient = httpClient;
            _s3Service = s3Service;
        }

        public async Task<int> CreateStoryAsync(CreateStoryRequest request, long userId)
        {
            try
            {
                var story = _mapper.Map<Story>(request);

                story.Slug = SlugGenerationHelper.GenerateSlug(request.Title);
                story.Slug = $"{story.Slug}-{RandomString(6)}"; // ensure unique
                story.Summary = SummaryHelper.Generate(request.Content);
                story.UserId = userId;

                if (request.StoryStatus == StoryStatus.Published)
                {
                    story.PublishedAt = DateTime.UtcNow;
                }

                if (request.IsAnonymous)
                {
                    story.IsAnonymous = true;
                }
                else
                {
                    story.IsAnonymous = false;
                }

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

                await _unitOfWork.SaveAsync();

                //// Merge audio files if any
                //if (request.AudioUrls != null && request.AudioUrls.Count > 0)
                //{
                //    string finalAudioKey;
                //    if (request.AudioUrls.Count == 1)
                //    {
                //        finalAudioKey = request.AudioUrls.First();
                //    }
                //    else
                //    {
                //        finalAudioKey = await MergeAudio(request.AudioUrls);
                //    }
                //    var media = await _mediaService.CreateAudioMediaAsync(story.Id, finalAudioKey);
                //    if (media > 0)
                //        await _userMediaService.AddUserMedia(userId, finalAudioKey, MediaType.Audio, UserMediaStatus.Confirmed);
                //}


                //// Add media urls
                //if (request.MediaUrls != null && request.MediaUrls.Count > 0)
                //{
                //    var media = await _mediaService.CreateMediaAsync(story.Id, request.MediaUrls);
                //    if (media > 0)
                //    {
                //        foreach (var url in request.MediaUrls)
                //        {
                //            await _userMediaService.AddUserMedia(userId, url, MediaType.Image, UserMediaStatus.Confirmed);
                //        }
                //    }
                //}

                if (request.MediaUrls?.Any(u => !string.IsNullOrWhiteSpace(u)) == true)
                {
                    await SyncUserMedia(userId, story.Id, request.MediaUrls, MediaType.Image);
                }

                if (request.AudioUrls?.Any(u => !string.IsNullOrWhiteSpace(u)) == true)
                {
                    await SyncUserMedia(userId, story.Id, request.AudioUrls, MediaType.Audio);
                }

                return story.Id;
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SyncUserMedia(long userId, int storyId, List<string> urls, MediaType type)
        {
            var existing = await _userMediaService.GetByUserAndUrls(userId, urls);

            if (type == MediaType.Audio)
            {
                // Nếu có nhiều audio → merge trước
                string finalAudioKey = urls.Count > 1
                    ? await MergeAudio(urls)
                    : urls.First();

                // Tạo mới cả Media và UserMedia
                var media = await _mediaService.CreateAudioMediaAsync(storyId, finalAudioKey);
                if (media > 0)
                    await _userMediaService.AddUserMedia(userId, finalAudioKey, MediaType.Audio, UserMediaStatus.Confirmed);
                
            }
            else if (type == MediaType.Image)
            {
                // Confirm hoặc add từng ảnh
                foreach (var url in urls)
                {
                    var media = await _mediaService.CreateMediaAsync(storyId, new List<string> { url });

                    var orphan = existing.FirstOrDefault(m => m.MediaUrl == url);                   
                    if (orphan != null)
                    {
                        orphan.Status = UserMediaStatus.Confirmed;
                        await _userMediaService.UpdateUserMedia(orphan);
                    }
                    else
                    {                      
                        if (media > 0)
                            await _userMediaService.AddUserMedia(userId, url, MediaType.Image, UserMediaStatus.Confirmed);
                    }
                }
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task<PaginatedResponse<GetStoryResponse>> GetStoriesByUserAsync(long userId, DateTime? cursor, long? userLikeId = null)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesByUserAsync(userId, cursor, false, true, false);
                var hasMore = stories.Count > 10;
                var items = stories.Take(10).Select(s =>
                {
                    var dto = _mapper.Map<GetStoryResponse>(s);
                    // user chưa login => false
                    dto.IsLiked = userLikeId != null && s.Likes.Any(l => l.UserId == userLikeId && l.RevokedAt == null);
                    dto.IsAI = false;
                    return dto;
                }).ToList();

                var nextCursor = hasMore ? items.Last().CreatedAt.ToString("o") : null;

                return new PaginatedResponse<GetStoryResponse>
                {
                    Items = items,
                    NextCursor = nextCursor,
                    HasMore = hasMore,
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PaginatedResponse<GetStoryResponse>> GetStoriesByOwnerAsync(long userId, DateTime? cursor, long userLikeId)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesByUserAsync(userId, cursor, true, true, false);
                var hasMore = stories.Count > 10;
                var items = stories.Take(10).Select(s =>
                {
                    var dto = _mapper.Map<GetStoryResponse>(s);
                    // user chưa login => false
                    dto.IsLiked = userLikeId != null && s.Likes.Any(l => l.UserId == userLikeId && l.RevokedAt == null);
                    dto.IsAI = false;
                    return dto;
                }).ToList();

                var nextCursor = hasMore ? items.Last().CreatedAt.ToString("o") : null;

                return new PaginatedResponse<GetStoryResponse>
                {
                    Items = items,
                    NextCursor = nextCursor,
                    HasMore = hasMore,
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PaginatedResponse<GetStoryResponse>> GetStoriesByUserAIAsync(long userId, DateTime? cursor, long? userLikeId = null)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesByUserAsync(userId, cursor, false, false, true);
                var hasMore = stories.Count > 10;
                var items = stories.Take(10).Select(s =>
                {
                    var dto = _mapper.Map<GetStoryResponse>(s);
                    // user chưa login => false
                    dto.IsLiked = userLikeId != null && s.Likes.Any(l => l.UserId == userLikeId && l.RevokedAt == null);
                    dto.IsAI = true;
                    return dto;
                }).ToList();

                var nextCursor = hasMore ? items.Last().CreatedAt.ToString("o") : null;

                return new PaginatedResponse<GetStoryResponse>
                {
                    Items = items,
                    NextCursor = nextCursor,
                    HasMore = hasMore,
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<PaginatedResponse<GetStoryResponse>> GetStoriesByOwnerAIAsync(long userId, DateTime? cursor, long userLikeId)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesByUserAsync(userId, cursor, true, false, true);
                var hasMore = stories.Count > 10;
                var items = stories.Take(10).Select(s =>
                {
                    var dto = _mapper.Map<GetStoryResponse>(s);
                    // user chưa login => false
                    dto.IsLiked = userLikeId != null && s.Likes.Any(l => l.UserId == userLikeId && l.RevokedAt == null);
                    dto.IsAI = true;
                    return dto;
                }).ToList();

                var nextCursor = hasMore ? items.Last().CreatedAt.ToString("o") : null;

                return new PaginatedResponse<GetStoryResponse>
                {
                    Items = items,
                    NextCursor = nextCursor,
                    HasMore = hasMore,
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PaginatedResponse<StoryResponse>> GetStoriesPreviewAsync(int limit, DateTime? cursor, long? userId = null)
        {
            try
            {
                var stories = await _storyRepository.GetStoriesPreviewAsync(limit, cursor);

                var hasMore = stories.Count > limit;
                var items = stories.Take(limit).Select(s =>
                {
                    var dto = _mapper.Map<StoryResponse>(s);

                    // user chưa login => false
                    dto.IsLiked = userId != null && s.Likes.Any(l => l.UserId == userId && l.RevokedAt == null);

                    return dto;
                }).ToList();
                
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

        public Task<Story> GetStoryByIdAsync(int storyId)
        {
            try
            {
                var story = _storyRepository.GetStoryByIdOrSlugAsync(storyId, null);
                return story;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GetStoryResponse?> GetStoryByIdOrSlugAsync(int? storyId, string? slug)
        {
            try
            {
                var story = await _storyRepository.GetStoryByIdOrSlugAsync(storyId, slug);
                return story == null ? null : _mapper.Map<GetStoryResponse>(story);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<GetStoryResponse?> GetStoryByIdOrSlugOwnerAsync(int? storyId, string? slug, long userId)
        {
            try
            {
                var story = await _storyRepository.GetStoryByIdOrSlugOwnerAsync(storyId, slug);
                if (story == null)
                    throw new Exception("Story not found");
                if (story.UserId != userId)
                    throw new Exception("You do not have permission to view this story");
                return story == null ? null : _mapper.Map<GetStoryResponse>(story);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> RemoveStoryAsync(int storyId, long userId)
        {
            try
            {
                var story = await _storyRepository.GetStoryByIdOrSlugAsync(storyId, null);
                if (story == null)
                    throw new Exception("Story not found");

                var isOwner = story.UserId == userId;
                if (!isOwner)
                    throw new Exception("You do not have permission to delete this story");

                _storyRepository.RemoveStory(story);
                var result = await _unitOfWork.SaveAsync();
                return result;
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
                var story = await _storyRepository.GetStoryByIdOrSlugUpdateAsync(storyId, null);
                if (story == null)
                    throw new Exception("Story not found");

                // Check user owns the story
                var isOwner = story.UserId == userId;
                if (!isOwner)
                    throw new Exception("You do not have permission to update this story");
               
                story.Title = request.Title;
                story.Slug = $"{SlugGenerationHelper.GenerateSlug(request.Title)}-{RandomString(6)}";
                story.Content = request.Content;
                story.Summary = SummaryHelper.Generate(request.Content);
                story.CoverImageUrl = string.IsNullOrWhiteSpace(request.CoverImageUrl)
                                        ? null
                                        : request.CoverImageUrl;
                story.PrivacyStatus = request.PrivacyStatus;
                story.StoryStatus = request.StoryStatus;
                story.LastUpdatedAt = DateTime.UtcNow;

                if (request.StoryStatus == StoryStatus.Published && story.PublishedAt == null)
                {
                    story.PublishedAt = DateTime.UtcNow;
                }

                if (request.StoryStatus != StoryStatus.Published)
                {
                    story.PublishedAt = null;
                }

                if (request.IsAnonymous)
                {
                    story.IsAnonymous = true;
                }
                else
                {
                    story.IsAnonymous = false;
                }

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

                var result = await _unitOfWork.SaveAsync();

                if (request.MediaUrls?.Any(u => !string.IsNullOrWhiteSpace(u)) == true)
                {
                    await SyncUserMedia(userId, story.Id, request.MediaUrls, MediaType.Image);
                }

                if (request.AudioUrls?.Any(u => !string.IsNullOrWhiteSpace(u)) == true)
                {
                    await SyncUserMedia(userId, story.Id, request.AudioUrls, MediaType.Audio);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<int> UpdateWithEntityAsync(Story story)
        {
            _storyRepository.UpdateStory(story);
            return _unitOfWork.SaveAsync();
        }

        private string RandomString(int seed)
        {
            string code = string.Concat(Guid.NewGuid().ToString("N")
                                .Where(char.IsDigit)
                                .Take(seed));

            return code;
        }

        private async Task<string> MergeAudio(List<string> audioUrls)
        {
            if (audioUrls == null || audioUrls.Count == 0)
            {
                throw new Exception("No audio files provided");
            }
            var tempFiles = new List<string>();
            
            try
            {
                // 1. Tải từng file về local tạm
                foreach (var url in audioUrls)
                {
                    var bytes = await _httpClient.GetByteArrayAsync($"https://cdn.storynest.io.vn/{url}");
                    var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                    await System.IO.File.WriteAllBytesAsync(tempFile, bytes);
                    tempFiles.Add(tempFile);
                }

                // 2. Merge ra MemoryStream
                using var mergedStream = MergeWavFilesToStream(tempFiles.ToArray());

                // 3. Upload lên S3
                var key = await _s3Service.UploadAIAudio(mergedStream);

                return key;
            }
            finally
            {
                // Cleanup file tạm
                foreach (var f in tempFiles)
                {
                    if (System.IO.File.Exists(f))
                        System.IO.File.Delete(f);
                }
            }
        }

        private MemoryStream MergeWavFilesToStream(params string[] sourceFiles)
        {
            var outputStream = new MemoryStream();
            byte[] header = null;
            int dataSize = 0;

            for (int i = 0; i < sourceFiles.Length; i++)
            {
                var bytes = System.IO.File.ReadAllBytes(sourceFiles[i]);

                if (i == 0)
                {
                    // Copy header từ file đầu tiên
                    header = new byte[44];
                    Array.Copy(bytes, 0, header, 0, 44);

                    // Ghi header tạm
                    outputStream.Write(header, 0, header.Length);
                    outputStream.Write(bytes, 44, bytes.Length - 44);

                    dataSize += bytes.Length - 44;
                }
                else
                {
                    // Các file sau bỏ header
                    outputStream.Write(bytes, 44, bytes.Length - 44);
                    dataSize += bytes.Length - 44;
                }
            }

            // Update header (ChunkSize & Subchunk2Size)
            outputStream.Seek(4, SeekOrigin.Begin);
            var bw = new BinaryWriter(outputStream, Encoding.ASCII, leaveOpen: true);

            int chunkSize = 36 + dataSize;
            int subChunk2Size = dataSize;

            bw.Write(chunkSize);

            outputStream.Seek(40, SeekOrigin.Begin);
            bw.Write(subChunk2Size);

            // Reset lại về đầu stream để đọc upload
            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream;
        }

        public async Task<StorySearchResult> SearchStoriesAsync(string keyword, int limit = 20, int? lastId = null)
        {
            try
            {
                var stories = await _storyRepository.SearchAsync(keyword, limit, lastId);

                return new StorySearchResult
                {
                    Stories = stories.Select(s => _mapper.Map<StoryResponse>(s)).ToList(),
                    LastId = stories.LastOrDefault()?.Id,
                    HasMore = stories.Count == limit
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
