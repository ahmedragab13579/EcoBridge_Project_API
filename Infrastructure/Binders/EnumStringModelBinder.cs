using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcoBridge.Infrastructure.Binders;

public class EnumStringModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;
            if (isNullable)
                bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var targetType = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;

        // Reject numeric input — enums must be bound by name, not underlying value.
        // This prevents silent breakage if enum values are ever renumbered.
        var trimmed = rawValue.Trim();
        if (long.TryParse(trimmed, out _))
        {
            AddError(bindingContext, trimmed, targetType);
            return Task.CompletedTask;
        }

        if (Enum.TryParse(targetType, trimmed, ignoreCase: true, out var parsedValue)
            && Enum.IsDefined(targetType, parsedValue))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedValue);
            return Task.CompletedTask;
        }

        AddError(bindingContext, trimmed, targetType);
        return Task.CompletedTask;
    }

    private static void AddError(ModelBindingContext context, string rawValue, Type enumType)
    {
        var names = Enum.GetNames(enumType);
        var allowed = string.Join(", ", names);
        context.ModelState.TryAddModelError(
            context.ModelName,
            $"The value '{rawValue}' is not valid for {enumType.Name}. Allowed values: {allowed}.");
    }
}