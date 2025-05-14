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
    private const string PROCESSOR_ID = "51ae6bdef6326124";

    private const string GEMINI_PROMPT = 
    """
    I will now send you the text of a bill (sometimes in Hebrew). I want you to analyze this text and extract the start date, the end date, and the total amount. Then, return this information to me in the following JSON structure: 
    json
    {
        "start_date": "YYYY-MM-DD",
        "end_date": "YYYY-MM-DD",
        "total_price": NUMBER
    }
    Here is the text:
    """;

    private readonly GroupService _groupService;

    public FileUploadService(GroupService groupService)
    {
        // Set the group service
        _groupService = groupService;

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
    public async Task<Document> ExtractDataWithProcessor(string filePath, string mimeType){
        // Create client
        try{
            var client = new DocumentProcessorServiceClientBuilder
            {
                Endpoint = $"{LOCATION_ID}-documentai.googleapis.com"
            }.Build();

            // Read in local file
            filePath = Path.Combine(_storagePath, filePath);
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
            var prompt = GEMINI_PROMPT + document.Text;
            Console.WriteLine(prompt);
            var extracted_data = await _groupService.ExtractDataFromTextWithGeminiAsync(prompt);
            return document;
            
        }catch(Exception e){
            Console.Write(e.Message);
            return new Document();
        }
    }
}
