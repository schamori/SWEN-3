namespace FileStorageService.Controllers
{
    public interface IFilesApi
    {
        Task UploadAsync(Stream fileStream, string fileName, string contentType);
        Task<byte[]> DownloadFromMinioAsync(string bucketName, string fileName);
    }

}
