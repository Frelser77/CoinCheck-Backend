using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LoginTamplate.ModelBinder
{
    public class CustomModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
            {
                return new CustomDecimalModelBinder();
            }

            return null;
        }
    }
}
