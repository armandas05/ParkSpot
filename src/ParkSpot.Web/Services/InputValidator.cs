using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Services.Interfaces;
namespace ParkSpot.Web.Services;

public class InputValidator : IInputValidator
{
    public string ValidateRequiredText(string? value, string fieldName, int maxLength)
    {
        value = value?.Trim();

        if (string.IsNullOrEmpty(value) || value.Length > maxLength)
            throw new DomainException($"{fieldName}: įveskite nuo 1 iki {maxLength} simbolių.");

        return value;
    }
}
