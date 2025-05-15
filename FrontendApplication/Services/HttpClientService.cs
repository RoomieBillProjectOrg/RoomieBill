using System;
using FrontendApplication.Services.Interfaces;

namespace FrontendApplication.Services;

public partial class HttpClientService : IHttpClientService
{
    public partial HttpMessageHandler GetPlatformSpecificHttpMessageHandler();
}
