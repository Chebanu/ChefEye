using FluentValidation;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ChefEye.Api.Extensions;

internal static class AbstractValidatorExtensions
{
    public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

    public const string AllowedPasswordCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+?.,";

    public static void RuleForUsername<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .Length(5, 20)
            .Must(x => x == null || x.All(c => AllowedUserNameCharacters.Contains(c)))
            .WithMessage($"Username must only contain the following characters: {AllowedUserNameCharacters}.");
    }

    public static void RuleForPassword<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .Length(8, 100)
            .Must(x => x == null || x.All(c => AllowedPasswordCharacters.Contains(c)))
            .WithMessage($"Password must only contain the following characters: {AllowedPasswordCharacters}.");
    }

    public static void RuleForOptionalEmail<T>(this AbstractValidator<T> validator, Expression<Func<T, string?>> expression)
    {
        validator.RuleFor(expression)
            .EmailAddress()
            .WithMessage("Please provide a valid email address.")
            .Length(5, 254)
            .WithMessage("Email must be between 5 and 254 characters long.")
            .Must(x => x == null || BeValidEmailDomain(x))
            .WithMessage("Email domain is not allowed.")
            .Must(x => x == null || NotContainConsecutiveDots(x))
            .WithMessage("Email cannot contain consecutive dots.")
            .When(x => !string.IsNullOrEmpty(GetPropertyValue(x, expression)));
    }

    public static void RuleForPhoneNumber<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Must(BeValidPhoneNumber)
            .WithMessage("Please provide a valid phone number. Supported formats: +1234567890, (123) 456-7890, 123-456-7890, 123.456.7890, 1234567890")
            .Length(10, 20)
            .WithMessage("Phone number must be between 10 and 20 characters long.");
    }
    private static bool BeValidEmailDomain(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        var blockedDomains = new[]
        {
            "junk.com"
        };

        var emailParts = email.Split('@');
        if (emailParts.Length != 2) return false;

        var domain = emailParts[1].ToLowerInvariant();
        return !blockedDomains.Contains(domain);
    }

    private static bool NotContainConsecutiveDots(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return !email.Contains("..");
    }

    private static bool BeValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return false;

        var digitsOnly = Regex.Replace(phoneNumber, @"[^\d+]", "");

        var phonePatterns = new[]
        {
            @"^\+\d{10,15}$",           // +1234567890
            @"^\d{10}$",                // 1234567890
            @"^\d{11}$",                // 11234567890
        };

        var formattedPatterns = new[]
        {
            @"^\+\d{1,3}[\s\-\.]?\(?\d{3}\)?[\s\-\.]?\d{3}[\s\-\.]?\d{4}$", // +1 (123) 456-7890
            @"^\(?\d{3}\)?[\s\-\.]?\d{3}[\s\-\.]?\d{4}$",                   // (123) 456-7890
            @"^\d{3}[\s\-\.]?\d{3}[\s\-\.]?\d{4}$",                         // 123-456-7890
        };

        return phonePatterns.Any(pattern => Regex.IsMatch(digitsOnly, pattern)) ||
               formattedPatterns.Any(pattern => Regex.IsMatch(phoneNumber, pattern));
    }

    private static string? GetPropertyValue<T>(T obj, Expression<Func<T, string?>> expression)
    {
        var compiled = expression.Compile();
        return compiled(obj);
    }
}
