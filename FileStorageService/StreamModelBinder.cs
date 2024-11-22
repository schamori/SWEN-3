using System.Net.Mime;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FileStorageService
{
    public class StreamModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Check if the content type is text/plain
            var contentType = bindingContext.HttpContext.Request.ContentType;
            if (!string.Equals(contentType, MediaTypeNames.Application.Octet, StringComparison.OrdinalIgnoreCase))
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            bindingContext.Result = ModelBindingResult.Success(bindingContext.HttpContext.Request.Body);

            return Task.CompletedTask;
        }
    }

}
