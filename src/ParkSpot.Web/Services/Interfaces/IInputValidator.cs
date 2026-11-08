namespace ParkSpot.Web.Services.Interfaces;

public interface IInputValidator
{
    string ValidateRequiredText(string? value, string fieldName, int maxLength);
}
