using System.ComponentModel.DataAnnotations;

namespace Prompter.Web.Validation;

/// <summary>
/// Validates that every string in the array is non-empty/whitespace and within the max length.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class ValidatePromptStringsAttribute(int maxLength = 4000) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string[] items)
            return ValidationResult.Success;

        for (var i = 0; i < items.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(items[i]))
                return new ValidationResult($"Prompt at index {i} is empty or whitespace.");

            if (items[i].Length > maxLength)
                return new ValidationResult($"Prompt at index {i} exceeds {maxLength} characters.");
        }

        return ValidationResult.Success;
    }
}
