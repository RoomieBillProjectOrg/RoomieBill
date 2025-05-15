using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FrontendApplication.Services.Interfaces;
using FrontendApplication.Models;
using System.Net.Http.Json;
using Google.Cloud.DocumentAI.V1;
using Newtonsoft.Json;


namespace FrontendApplication.Services;
public class UploadServiceApi : IUploadServiceApi
{
    private readonly HttpClient _httpClient;

    public UploadServiceApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }

    public async Task<string> UploadReceiptAsync(string filePath)
    {   
        using var content = new MultipartFormDataContent();
        
        await using var fileStream = File.OpenRead(filePath);
        using var streamContent = new StreamContent(fileStream);
        
        // Set correct Content-Type for file stream
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        // Ensure the form key name "file" matches the server-side parameter
        content.Add(streamContent, "file", Path.GetFileName(filePath));
        
        // Ensure correct API route
        var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Upload/upload", content);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<UploadResponse>(jsonResponse);
        
        return result.FileName;
    }

    public async Task<Stream> DownloadReceiptAsync(string fileName)
    {
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Upload/download/{fileName}");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<BillData> ExtractData(string fileName)
    {   
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Upload/extract/{fileName}");
        response.EnsureSuccessStatusCode();
        var bill = await response.Content.ReadFromJsonAsync<BillData>();
        return bill;
    }
    private class UploadResponse
    {
        public string FileName { get; set; }
    }

}
