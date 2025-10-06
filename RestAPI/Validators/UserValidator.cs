using FluentValidation;
using RestAPI.Constants;
using RestAPI.Models;
using RestAPI.Validators;

namespace RestAPI.Validators
{
    public class UserValidator : AbstractValidatorBase<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldName))
                .Length(2, 50).WithMessage(FormatMessage(ValidationMessages.StringLength, ValidationMessages.FieldName, 2, 50))
                .Matches("^[a-zA-Z ]+$").WithMessage(ValidationMessages.NameAlphaOnly);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldEmail))
                .EmailAddress().WithMessage(ValidationMessages.EmailInvalid)
                .Must(BeUniqueEmail).WithMessage(ValidationMessages.EmailAlreadyExists);

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 100)
                .WithMessage(FormatMessage(ValidationMessages.AgeRange, 18, 100));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldPassword))
                .MinimumLength(8).WithMessage(ValidationMessages.PasswordMinLength)
                .Matches("[A-Z]").WithMessage(ValidationMessages.PasswordUppercase)
                .Matches("[a-z]").WithMessage(ValidationMessages.PasswordLowercase)
                .Matches("[0-9]").WithMessage(ValidationMessages.PasswordDigit)
                .Matches("[^a-zA-Z0-9]").WithMessage(ValidationMessages.PasswordSpecialChar);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(ValidationMessages.PasswordsDoNotMatch);

            RuleFor(x => x.CreatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ValidationMessages.InvalidCreationDate);
        }

        private bool BeUniqueEmail(string email)
        {
            return !email.EndsWith("@existing.com");
        }
    }

    public class CreateUserRequestValidator : AbstractValidatorBase<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldName))
                .Length(2, 50).WithMessage(FormatMessage(ValidationMessages.StringLength, ValidationMessages.FieldName, 2, 50));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldEmail))
                .EmailAddress().WithMessage(ValidationMessages.EmailInvalid);

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 100)
                .WithMessage(FormatMessage(ValidationMessages.AgeRange, 18, 100));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldPassword))
                .MinimumLength(8).WithMessage(ValidationMessages.PasswordMinLength);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(ValidationMessages.PasswordsDoNotMatch);
        }
    }
}