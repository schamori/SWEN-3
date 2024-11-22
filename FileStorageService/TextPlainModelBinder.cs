using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FileStorageService
{
    public class TextPlainModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Check if the content type is text/plain
            var contentType = bindingContext.HttpContext.Request.ContentType;
            if (!string.Equals(contentType, "text/plain", StringComparison.OrdinalIgnoreCase))
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            // Read the request body as string
            using (var reader = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                bindingContext.Result = ModelBindingResult.Success(body);
            }
        }
    }
}
