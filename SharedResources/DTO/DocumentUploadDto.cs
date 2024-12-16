using Microsoft.AspNetCore.Http;

namespace SharedResources.DTO
{
    public class DocumentUploadDto
    {
        public IFormFile File { get; set; }
    }
}
