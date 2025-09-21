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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                    ParentCommentId = request.ParentCommentId.HasValue && request.ParentCommentId.Value > 0
                                            ? request.ParentCommentId
                                            : null,
                    CommentStatus = CommentStatus.Active
                };

                if (request.ParentCommentId.HasValue)
                {
                    var parent = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
                    if (parent == null || parent.StoryId != storyId)
                        throw new ArgumentException("Invalid parent comment id");
                }

                await _commentRepository.AddAsync(comment);
                await _unitOfWork.SaveAsync();

                var newComment = await _commentRepository.GetByIdAsync(comment.Id);
                var response = _mapper.Map<CommentResponse>(newComment);

                response.HasReplies = false;

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message);
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

        public async Task<List<CommentResponse>> GetCommentsAsync(int story, int? parentId, int limit, int offset)
        {
            try
            {
                var comments = await _commentRepository.GetByStoryId(story, parentId, limit, offset);
                var ids = comments.Select(c => c.Id).ToList();
                var repliesCount = await _commentRepository.GetRepliesCount(ids);


                var response = _mapper.Map<List<CommentResponse>>(comments);

                if (response == null || response.Count == 0)
                    return new List<CommentResponse>();

                foreach (var comment in response)
                {
                    if (repliesCount.TryGetValue(comment.Id, out var count))
                    {
                        comment.RepliesCount = count;
                        comment.HasReplies = count > 0;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
