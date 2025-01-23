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

//get group by id
    public async Task<GroupModel> GetGroup(int groupId)
    {   
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getGroup?id={groupId}");
        response.EnsureSuccessStatusCode();
        
        var group = await response.Content.ReadFromJsonAsync<GroupModel>();
        return group ?? new GroupModel(); // Return an empty group if the deserialization results in null
    }

    public async Task<List<DebtModel>> GetDebtsForUserAsync(int groupId, int userId){
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getDebtsForUser?groupId={groupId}&userId={userId}");
        response.EnsureSuccessStatusCode();
        
        var debts = await response.Content.ReadFromJsonAsync<List<DebtModel>>();
        return debts ?? new List<DebtModel>(); // Return an empty list if the deserialization results in null
    }

    public async Task<List<DebtModel>>  GetDebtsOwedByUserAsync(int groupId, int userId){
        var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getDebtsOwedByUser?groupId={groupId}&userId={userId}");
        response.EnsureSuccessStatusCode();
        
        var debts = await response.Content.ReadFromJsonAsync<List<DebtModel>>();
        return debts ?? new List<DebtModel>(); // Return an empty list if the deserialization results in null
    }
}
