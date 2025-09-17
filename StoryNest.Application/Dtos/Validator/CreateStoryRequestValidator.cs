using FluentValidation;
using StoryNest.Application.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Validator
{
    public class CreateStoryRequestValidator : AbstractValidator<CreateStoryRequest>
    {
        public CreateStoryRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(150).WithMessage("Title must not exceed 150 characters");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required")
                .MaximumLength(20000).WithMessage("Content too long (max 20k chars)");

            RuleFor(x => x.Summary)
                .MaximumLength(300).WithMessage("Summary too long (max 300 chars)")
                .When(x => !string.IsNullOrEmpty(x.Summary));

            RuleFor(x => x.CoverImageUrl)
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.CoverImageUrl))
                .WithMessage("CoverImageUrl must be a valid URL");

            RuleFor(x => x.Tags)
                .NotEmpty().WithMessage("At least 1 tag is required")
                .Must(tags => tags.Count <= 5).WithMessage("Maximum 5 tags allowed")
                .ForEach(tag =>
                {
                    tag.NotEmpty().WithMessage("Tag cannot be empty")
                       .MaximumLength(30).WithMessage("Tag must not exceed 30 characters");
                });
        }
        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
