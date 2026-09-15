using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcoBridge.Infrastructure.Binders;

public class EnumStringModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.UnderlyingOrModelType;
        if (modelType.IsEnum)
        {
            return new EnumStringModelBinder();
        }

        return null;
    }
}
