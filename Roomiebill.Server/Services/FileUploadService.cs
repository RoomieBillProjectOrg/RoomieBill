using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Roomiebill.Server.Services;

public class FileUploadService
{
    private readonly string _storagePath;

    public FileUploadService()
    {
        // Change this path later when moving to the server
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "StoredFiles");

        // Ensure the directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file.");

        Guid receiptGuid = Guid.NewGuid();
        string fileNameWithGuid = $"{receiptGuid}_{file.FileName}";
        string filePath = Path.Combine(_storagePath, fileNameWithGuid);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileNameWithGuid;
    }

	public async Task<byte[]> GetFileAsync(string fileNameWithGuid)
	{
		string filePath = Path.Combine(_storagePath, fileNameWithGuid);
		if (!File.Exists(filePath))
			throw new FileNotFoundException("File not found.");

		return await File.ReadAllBytesAsync(filePath);
	}
}
