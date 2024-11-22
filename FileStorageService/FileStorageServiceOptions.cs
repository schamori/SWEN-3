namespace FileStorageService
{
    public class FileStorageServiceOptions
    {
        public string Endpoint { get; set; } = "http://minio:9000";
        public string AccessKey { get; set; } = "minioadmin";
        public string SecretKey { get; set; } = "minioadmin";
        public string BucketName { get; set; } = "documents";
    }
}
