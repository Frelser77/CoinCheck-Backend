using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace LoginTamplate.ModelBinder
{
    public class CustomDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None && !string.IsNullOrEmpty(valueProviderResult.FirstValue))
            {
                string value = valueProviderResult.FirstValue;
                if (decimal.TryParse(value, NumberStyles.Any, new CultureInfo("it-IT"), out decimal result))
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid decimal format.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
