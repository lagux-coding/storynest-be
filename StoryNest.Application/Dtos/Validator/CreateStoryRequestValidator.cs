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
                .NotEmpty().WithMessage("Title is required");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required");

            RuleFor(x => x.Tags)
                .ForEach(tag =>
                {
                    tag.NotEmpty().WithMessage("Tag cannot be empty")
                       .MaximumLength(30).WithMessage("Tag must not exceed 30 characters");
                });
        }
    }
}
