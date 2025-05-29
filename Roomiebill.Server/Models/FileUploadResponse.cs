namespace Roomiebill.Server.Models
{
    public class FileUploadResponse
    {
        public string FileName { get; set; }

        public FileUploadResponse(string fileName)
        {
            FileName = fileName;
        }
    }
}
