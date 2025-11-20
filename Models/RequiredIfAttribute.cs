using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models;

public class RequiredIfAttribute : ValidationAttribute
{
    private string PropertyName { get; set; }
    private object DesiredValue { get; set; }

    public RequiredIfAttribute(string propertyName, object desiredValue)
    {
        PropertyName = propertyName;
        DesiredValue = desiredValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(PropertyName)?.GetValue(instance, null);

        if (propertyValue?.ToString() == DesiredValue.ToString())
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}