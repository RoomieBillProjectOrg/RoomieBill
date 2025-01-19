using System;

namespace FrontendApplication.Services;

public class GroupServiceApi
{
    private readonly HttpClient _httpClient;

    public GroupServiceApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }
}
