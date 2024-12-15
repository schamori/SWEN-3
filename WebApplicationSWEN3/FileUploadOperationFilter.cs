using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using SharedResources.DTO;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using SharedResources.DTO;

namespace WebApplicationSWEN3
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasFile = context.MethodInfo
                .GetParameters()
                .Any(p => p.ParameterType == typeof(DocumentUploadDto));

            if (!hasFile)
                return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["File"] = new OpenApiSchema { Type = "string", Format = "binary" }
                            },
                            Required = new HashSet<string> { "File" }
                        }
                    }
                }
            };
        }
    }
}
