using FluentValidation;
using RestAPI.Constants;
using RestAPI.Models;
using RestAPI.Validators;

namespace RestAPI.Validators
{
    public class AddressValidator : AbstractValidatorBase<Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldStreet))
                .MaximumLength(100).WithMessage(ValidationMessages.StreetTooLong);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldCity))
                .MaximumLength(50).WithMessage(ValidationMessages.CityTooLong);

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldCountry))
                .MaximumLength(50).WithMessage(ValidationMessages.CountryTooLong);

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldPostalCode))
                .Matches(@"^\d{5}(-\d{4})?$").WithMessage(ValidationMessages.InvalidPostalCode);
        }
    }

    public class CustomerValidator : AbstractValidatorBase<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldName))
                .MaximumLength(100).WithMessage(ValidationMessages.NameTooLong);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(FormatMessage(ValidationMessages.RequiredField, ValidationMessages.FieldEmail))
                .EmailAddress().WithMessage(ValidationMessages.EmailInvalid);

            RuleFor(x => x.Address).SetValidator(new AddressValidator());

            RuleFor(x => x.ShippingAddress).SetValidator(new AddressValidator())
                .When(x => x.ShippingAddress != null);
        }
    }
}