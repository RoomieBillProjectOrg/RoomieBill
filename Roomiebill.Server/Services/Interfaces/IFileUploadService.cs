using Microsoft.AspNetCore.Http;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> SaveFileAsync(IFormFile file);
        Task<byte[]> GetFileAsync(string fileNameWithGuid);
    }
}
