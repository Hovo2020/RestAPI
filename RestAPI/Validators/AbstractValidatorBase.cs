using FluentValidation;

namespace RestAPI.Validators
{
    public abstract class AbstractValidatorBase<T> : AbstractValidator<T>
    {
        protected string FormatMessage(string message, params object[] args)
        {
            return args.Length > 0 ? string.Format(message, args) : message;
        }
    }
}