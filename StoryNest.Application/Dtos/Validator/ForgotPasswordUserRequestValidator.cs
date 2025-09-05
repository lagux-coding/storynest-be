using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Validator
{
    public class ForgotPasswordUserRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress(EmailValidationMode.AspNetCoreCompatible).WithMessage("Invalid email format.");
        }
    }
}
