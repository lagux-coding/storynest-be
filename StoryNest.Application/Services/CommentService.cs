using AutoMapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
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
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IStoryService _storyService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IUnitOfWork unitOfWork, IMapper mapper, IStoryService storyService, INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _storyService = storyService;
            _notificationService = notificationService;
        }

        public async Task<CommentResponse> AddCommentAsync(CreateCommentRequest request, int storyId, long userId)
        {
            try
            {
                var comment = new Comment
                {
                    StoryId = storyId,
                    UserId = userId,
                    Content = request.Content,
                    ParentCommentId = request.ParentCommentId > 0
                                            ? request.ParentCommentId
                                            : null,
                    CommentStatus = CommentStatus.Active,
                    IsAnonymous = request.IsAnonymous
                };

                if (request.ParentCommentId > 0)
                {
                    var parent = await _commentRepository.GetByIdAsync(request.ParentCommentId);
                    if (parent == null || parent.StoryId != storyId)
                        throw new ArgumentException("Invalid parent comment id");
                }                             

                await _commentRepository.AddAsync(comment);
                await _unitOfWork.SaveAsync();

                var story = await _storyService.GetStoryByIdAsync(storyId);
                if (story == null)
                    throw new ArgumentException("Invalid story id");
                var commentCount = await _commentRepository.CountComments(storyId);

                story.CommentCount = commentCount;
                await _storyService.UpdateWithEntityAsync(story);

                var newComment = await _commentRepository.GetByIdAsync(comment.Id);
                var response = _mapper.Map<CommentResponse>(newComment);

                response.IsOwner = true;
                response.HasReplies = false;

                if (story.UserId != userId)
                {
                    await _notificationService.SendNotificationAsync(story.UserId, userId, $"<strong>{response.User.Username}</strong> vừa để lại vài dòng cảm xúc trong câu chuyện <strong>{story.Title}</strong> của bạn.", NotificationType.StoryCommented, story.Id, "Story", newComment.IsAnonymous);
                }

                if (newComment.ParentCommentId.HasValue && newComment.ParentCommentId.Value > 0)
                {
                    var parent = await _commentRepository.GetByIdAsync(newComment.ParentCommentId.Value);
                    if (parent != null && parent.UserId != userId)
                    {
                        await _notificationService.SendNotificationAsync(
                            parent.UserId,
                            userId,
                            $"<strong>{response.User.Username}</strong> đã phản hồi cảm xúc của bạn trong câu chuyện <strong>{story.Title}</strong>.",
                            NotificationType.StoryCommented,
                            parent.Id,
                            "comment",
                            newComment.IsAnonymous
                        );
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId, long userId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null || comment.UserId != userId)
                    return false;

                comment.CommentStatus = CommentStatus.Deleted;
                comment.DeletedAt = DateTime.UtcNow;

                await _commentRepository.UpdateAsync(comment);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginatedResponse<CommentResponse>> GetCommentsAsync(int storyId, long userId, int? parentId, int limit, int? cursor)
        {
            try
            {
                var comments = await _commentRepository.GetByStoryId(storyId, parentId, limit, cursor);
                var hasMore = comments.Count > limit;

                if (hasMore)
                    comments = comments.Take(limit).ToList();

                var ids = comments.Select(c => c.Id).ToList();
                var repliesCount = await _commentRepository.GetRepliesCount(ids);
                var response = _mapper.Map<List<CommentResponse>>(comments);

                foreach (var comment in response)
                {
                    if (repliesCount.TryGetValue(comment.Id, out var count))
                    {
                        comment.RepliesCount = count;
                        comment.HasReplies = count > 0;
                    }

                    if (comment.CommentStatus == CommentStatus.Deleted)
                        comment.Content = "[deleted]";

                    if (comment.UserId == userId)
                    {
                        comment.IsOwner = true;
                    }
                }

                var nextCursor = response.LastOrDefault()?.Id;

                return new PaginatedResponse<CommentResponse>
                {
                    Items = response,
                    HasMore = hasMore,
                    NextCursor = hasMore ? nextCursor?.ToString() : null
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Comment>> GetCommentByUserAsync(long userId, int limit, int cursor = 0)
        {
            try
            {
                var comments = await _commentRepository.GetByUserId(userId, limit, cursor);
                return comments ?? new List<Comment>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> UpdateCommentAsync(CreateCommentRequest request, int commentId, long userId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null || comment.UserId != userId)
                    return false;

                comment.Content = request.Content;
                comment.UpdatedAt = DateTime.UtcNow;

                await _commentRepository.UpdateAsync(comment);
                await _unitOfWork.SaveAsync();

                var story = await _storyService.GetStoryByIdAsync(comment.StoryId);
                if (story == null)
                    throw new ArgumentException("Invalid story id");
                var commentCount = await _commentRepository.CountComments(story.Id);

                story.CommentCount = commentCount;
                await _storyService.UpdateWithEntityAsync(story);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Story?> GetStoryByCommentAsync(int commentId)
        {
            try
            {
                var story = await _commentRepository.GetStoryByCommentAsync(commentId);
                return story ?? null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
