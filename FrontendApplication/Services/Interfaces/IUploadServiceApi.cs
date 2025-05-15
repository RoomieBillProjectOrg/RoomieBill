namespace FrontendApplication.Services.Interfaces
{
    public interface IUploadServiceApi
    {
        Task<string> UploadReceiptAsync(string filePath);
        Task<Stream> DownloadReceiptAsync(string fileName);
    }
}
