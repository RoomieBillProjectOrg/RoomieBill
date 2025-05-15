using System.Net.Http;

namespace FrontendApplication.Services.Interfaces
{
    public interface IHttpClientService
    {
        HttpMessageHandler GetPlatformSpecificHttpMessageHandler();
    }
}
