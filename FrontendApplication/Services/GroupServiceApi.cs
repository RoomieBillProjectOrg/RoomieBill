using System;
using System.Net.Http.Json;
using FrontendApplication.Models;

namespace FrontendApplication.Services;

public class GroupServiceApi
{
    private readonly HttpClient _httpClient;

    public GroupServiceApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }

    public async Task<List<GroupModel>> GetUserGroups(UserModel user)
    {
        //TODO: maybe exception catch needed
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getUserGroups?UserId={user.Id}");
        response.EnsureSuccessStatusCode();
        
        var groups = await response.Content.ReadFromJsonAsync<List<GroupModel>>();
        return groups ?? new List<GroupModel>(); // Return an empty list if the deserialization results in null
    }
}
