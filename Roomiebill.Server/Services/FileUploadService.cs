using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;

namespace Roomiebill.Server.Services;

public class FileUploadService
{
    private readonly string _storagePath;
    private const string PROJECT_ID = "august-craft-457418-g9";
    private const string LOCATION_ID = "eu";
    private const string PROCESSOR_ID = "ce7ccb1a42598988";

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

    // Extract data with processor
    public Document ExtractDataWithProcessor(string filePath, string mimeType){
        // Create client
        try{
            var client = new DocumentProcessorServiceClientBuilder
            {
                Endpoint = $"{LOCATION_ID}-documentai.googleapis.com"
            }.Build();

        // Read in local file
        using var fileStream = File.OpenRead(filePath);
        var rawDocument = new RawDocument
        {
            Content = ByteString.FromStream(fileStream),
            MimeType = mimeType
        };

        // Initialize request argument(s)
        var request = new ProcessRequest
        {
            Name = ProcessorName.FromProjectLocationProcessor(PROJECT_ID, LOCATION_ID, PROCESSOR_ID).ToString(),
            RawDocument = rawDocument
        };

        // Make the request
        var response = client.ProcessDocument(request);

        var document = response.Document;
        Console.WriteLine(document.Text);
        return document;
        }catch(Exception e){
            Console.Write(e.Message);
            return new Document();
        }
    }
}
