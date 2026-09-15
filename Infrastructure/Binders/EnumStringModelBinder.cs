using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcoBridge.Infrastructure.Binders;

public class EnumStringModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;
            if (isNullable)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            return Task.CompletedTask;
        }

        var targetType = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;

        if (Enum.TryParse(targetType, rawValue, ignoreCase: true, out var parsedValue) && Enum.IsDefined(targetType, parsedValue))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedValue);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"The value '{rawValue}' is not valid for {targetType.Name}.");

        return Task.CompletedTask;
    }
}
