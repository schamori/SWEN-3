namespace FileStorageService.Controllers
{
    public interface IFilesApi
    {
        Task UploadAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadFromMinioAsync(string bucketName, string fileName);
    }

}
