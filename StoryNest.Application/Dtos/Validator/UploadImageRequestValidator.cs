using FluentValidation;
using StoryNest.Application.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Validator
{
    public class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
    {
        private static readonly string[] _allowedResourceTypes = { "avatar", "cover-story", "cover-user", "story", "comment" };
        private static readonly string[] _allowedContentTypes = { "image/png", "image/jpeg", "image/jpg", "image/webp", "image/gif", "image/webp", "image/bmp", "image/svg+xml", "image/avif" };
        private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

        public UploadImageRequestValidator()
        {
            RuleFor(x => x.ResourceType)
                .NotEmpty().WithMessage("ResourceType is required.")
                .Must(type => _allowedResourceTypes.Contains(type.ToLower()))
                .WithMessage($"ResourceType must be one of: {string.Join(", ", _allowedResourceTypes)}");

            RuleFor(x => x.ResourceId)
                .GreaterThan(0).WithMessage("ResourceId must be a positive integer.");

            RuleForEach(x => x.Files)
                .SetValidator(new FileMetadataValidator());

            When(x => x.ResourceType != null && !x.ResourceType.Equals("avatar", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.ResourceId)
                    .NotNull().WithMessage("ResourceId is required for non-avatar uploads.");
            });
        }

        private class FileMetadataValidator : AbstractValidator<FileMetadata>
        {
            public FileMetadataValidator()
            {
                RuleFor(f => f.ContentType)
                    .NotEmpty().WithMessage("ContentType is required.")
                    .Must(ct => _allowedContentTypes.Contains(ct.ToLower()))
                    .WithMessage($"Only content types allowed: {string.Join(", ", _allowedContentTypes)}");

                RuleFor(f => f.FileSize)
                    .GreaterThan(0).WithMessage("File size must be greater than 0.")
                    .LessThanOrEqualTo(MaxFileSize).WithMessage($"File size must not exceed {MaxFileSize / 1024 / 1024} MB.");
            }
        }
    }
}
