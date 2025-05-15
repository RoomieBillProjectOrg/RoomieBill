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
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Receipt file not found.", filePath);
            }

            using var content = new MultipartFormDataContent();
            
            try
            {
                await using var fileStream = File.OpenRead(filePath);
                using var streamContent = new StreamContent(fileStream);
                
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "file", Path.GetFileName(filePath));
                
                var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Upload/upload", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Upload failed: {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UploadResponse>(jsonResponse);
                
                if (result?.FileName == null)
                {
                    throw new Exception("Failed to get uploaded file name from server response.");
                }

                return result.FileName;
            }
            catch (IOException ex)
            {
                throw new Exception("Failed to read the receipt file. The file might be in use or corrupted.", ex);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to upload receipt. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the upload response. Please try again.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Upload failed:"))
        {
            throw; // Re-throw upload-specific errors as they are already well-formatted
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while uploading the receipt. Please try again.", ex);
        }
    }

    public async Task<Stream> DownloadReceiptAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required.", nameof(fileName));
            }

            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Upload/download/{fileName}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Download failed: {errorContent}");
            }

            return await response.Content.ReadAsStreamAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to download receipt. Please check your internet connection.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Download failed:"))
        {
            throw; // Re-throw download-specific errors as they are already well-formatted
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while downloading the receipt. Please try again.", ex);
        }
    }

    public async Task<BillData> ExtractData(string fileName)
    {   
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required for data extraction.", nameof(fileName));
            }

            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Upload/extract/{fileName}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Data extraction failed: {errorContent}");
            }

            var bill = await response.Content.ReadFromJsonAsync<BillData>();
            return bill ?? throw new Exception("Failed to extract bill data from the receipt.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to connect to the data extraction service. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the extracted data. The receipt might be unclear or in an unsupported format.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Data extraction failed:"))
        {
            throw; // Re-throw extraction-specific errors as they are already well-formatted
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while extracting data from the receipt. Please try again.", ex);
        }
    }
    private class UploadResponse
    {
        public string FileName { get; set; }
    }

}
